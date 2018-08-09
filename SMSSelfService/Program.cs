using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Security;
using ADLibrary;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using HelperLibrary;
using NLog;
using NLog.Targets;
using PhoneNumbers;
using XMLConfig;
using Timer = System.Timers.Timer;

namespace SMSSelfService
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int ModemBaud = 19200; // baudrate for modem
        private const int ModemTimeout = 300; // timeout for modem
        private static GsmCommMain _comm;

        private static ActiveDirectory _activeDirectory;
        private static MailHelper _mailHelper;
        private static Config _config;
        private static PhoneNumberUtil _phoneNumberUtil;

        private static Dictionary<string, int> _intrusionsList;

        private static Timer _infotimer;

        //Supplied to phone number verification as default region, loaded from config
        private static string _currentPhoneRegion;

        #region Nested classes to support running as service
        public const string ServiceName = "smsselfservice";

        public class Service : ServiceBase
        {
            private Thread mainthread;
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                ThreadStart thstart = () => AppBody(args);
                mainthread = new Thread(thstart);
                mainthread.Start();
            }

            protected override void OnStop()
            {
                mainthread.Abort();
            }
        }
        #endregion

        static void Main(string[] args)
        {
            //Setup exception handler if we are not being debugged
            if (!AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe"))
            {
                // Add the event handler for handling non-UI thread exceptions to the event. 
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;                
            }


            if (!Environment.UserInteractive)
            {
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            }
            else
            {
                // running as console app
                AppBody(args);

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception) e.ExceptionObject;
            _logger.Fatal($"FATAL ERROR, APPLICATION WILL BE CLOSED. PLEASE SEND A BUG REPORT TO DEVELOPER. ERROR: {ex}");

            try
            {
                SendMail("SMS Self Service - FATAL ERROR", $"<h2><font color=\"red\">FATAL ERROR, APPLICATION WILL BE CLOSED. PLEASE SEND A BUG REPORT TO DEVELOPER. ERROR:</font></h2><br>{ex}");
            }
            catch (Exception) {}
            Environment.Exit(2);
        }

        //This method contains main application code executed by console application's Main() and service's OnStart()
        private static void AppBody(string[] args)
        {
            FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            _logger.Info($"SMS Self Service {ver.FileMajorPart}.{ver.FileMinorPart} Build {ver.FilePrivatePart}");
            _logger.Info("Copyright (c) 2015-2018 Aleksey Tsutsey");

            _logger.Debug("Initializing...");

            _intrusionsList = new Dictionary<string, int>();

            _config = new Config($"{Utils.ApplicationLocationPath()}\\config.xml", true);

            _mailHelper = new MailHelper(
                _config.GetValue("Mail/Host", ""),
                _config.GetValue("Mail/UseSSL", true),
                _config.GetValue("Mail/Username", ""),
                _config.GetValue("Mail/Password", "")
            );

            _phoneNumberUtil = PhoneNumberUtil.GetInstance();
            _currentPhoneRegion = _config.GetValue("Protection/Spam/PhoneRegion", "ZZ");

            _logger.Debug($"Current phone region: {_currentPhoneRegion}");

            //Initialize modem status print timer
            if (_config.GetValue("General/InformationTimer/Enabled", true))
            {
                _infotimer = new Timer(_config.GetValue("General/InformationTimer/Interval", 60) * 1000);
                _infotimer.Elapsed += infotimer_Elapsed;
                _infotimer.Start();
            }

            string logpath = _config.GetValue("General/LogsDirectory", $"{Utils.ApplicationLocationPath()}\\logs");
            if (Directory.Exists(logpath))
            {
                foreach (FileTarget target in LogManager.Configuration.AllTargets.Where(t => t is FileTarget))
                {
                    var filename = Path.GetFileName(target.FileName.ToString().Trim('\''));
                    target.FileName = Path.Combine(logpath, filename);
                }
            }
            else
            {
                _logger.Warn("Specified log directory doesn't exist, using default...");
            }

            _logger.Debug("Loading configuration...");

            string controllerAddress = _config.GetValue("Domain/ControllerAddress", "");
            string container = _config.GetValue("Domain/Container", "");

            if (string.IsNullOrEmpty(controllerAddress) || string.IsNullOrEmpty(container))
            {
                _logger.Fatal("Invalid Domain Controller settings");
                return;
            }

            _logger.Debug("Connecting to domain controller...");

            //Initialize encryption and decrypt admin password if it exists
            string encyptedAdminPassword = _config.GetValue("Domain/AdminPassword", "");
            string adminPassword = "";

            if (!string.IsNullOrEmpty(encyptedAdminPassword))
            {
                try
                {
                    EncryptionHelper encryptionHelper =
                        new EncryptionHelper(_config.GetValue("Encryption/Entropy", ""));

                    if (encryptionHelper.IsEntropyGenerated)
                        _config.SetValue("Encryption/Entropy", encryptionHelper.Entropy);

                    adminPassword = encryptionHelper.Decrypt(encyptedAdminPassword);
                }
                catch (Exception ex)
                {
                    _logger.Error("Unable to decrypt administrator password, application will be closed.");
                    _logger.Debug($"Exception while decrypting admin password: {ex}");
                    return;
                }
            }

            _activeDirectory = new ActiveDirectory(controllerAddress, container, _config.GetValue("Domain/AdminUsername", ""), adminPassword);

            int crashes = 0;
            DateTime lastcrashtime = DateTime.Now;
            while (true)
            {
                _logger.Debug("Starting polling loop");

                ModemPollingLoop();

                _logger.Debug("Polling loop crashed");

                if (DateTime.Now.Subtract(lastcrashtime).TotalMinutes > 10)
                    crashes = 0;
                crashes += 1;
                _logger.Debug($"Total crashes: {crashes}");
                _logger.Debug($"Time between crashes: {DateTime.Now.Subtract(lastcrashtime).TotalMinutes}");

                //More than 5 loop exits in last 5 minutes? Exit
                if (crashes >= 5 && DateTime.Now.Subtract(lastcrashtime).TotalMinutes < 5)
                    break;
            }

            _logger.Fatal("Unable to establish modem connection, server shutting down...");
            SendMail("SMS Self Service Server - Fatal Error",
                "<h2><font color=\"red\">Unable to establish modem connection, server shutting down.</font></h2>");

            Environment.Exit(1);
        }

        private static void ModemPollingLoop()
        {
            //Find a port where the modem is at
            _logger.Info("Searching for a modem...");
            for (int i = 1; i <= 256; i++)
            {
                if (TestConnection($"COM{i}"))
                    break;
            }

            if (_comm.IsOpen() && _comm.IsConnected())
            {
                _logger.Info("Modem found");
                _logger.Info("Setting modem settings...");
                _comm.SelectCharacterSet("ucs2"); //Use UTF-16 encoding when getting data from modem

                //Hook debugging event if user wants to debug modem commands
                if(_config.GetValue("Debug/ModemCommands",false))
                    _comm.LoglineAdded += comm_LoglineAdded;

                try
                {
                    _comm.SetSmscAddress(_config.GetValue("General/SMSCenter", ""));
                }
                catch (Exception)
                {
                    _logger.Warn("Invalid SMS Center number or this feature is not supported by current modem");
                }

                _logger.Info("Listening for incoming messages...");
                while (_comm.IsOpen())
                {
                    if (_comm.IsConnected())
                    {
                        try
                        {
                            //Get all messages saved on SIM
                            DecodedShortMessage[] messages = _comm.ReadMessages(PhoneMessageStatus.All,
                                PhoneStorageType.Sim);

                            foreach (DecodedShortMessage message in messages)
                            {
                                SmsPdu pdu = message.Data;
                                if (pdu is SmsDeliverPdu) // Check if it's SMS and not some kind of a service message
                                {
                                    SmsDeliverPdu data = (SmsDeliverPdu) pdu;
                                    _logger.Debug($"Received message from {data.OriginatingAddress}: " +
                                                data.UserDataText + $" [TIMESTAMP: {data.SCTimestamp.ToSortableString()}]");

                                    ChangePassword(data.OriginatingAddress, data.UserDataText);

                                    //Delete message so it won't be parsed again 
                                    //and so we don't have a problem with free space on SIM
                                    //in the future
                                    _comm.DeleteMessage(message.Index, message.Storage);
                                }
                                else
                                {
                                    _logger.Warn($"Received unknown message type: {pdu.GetType()}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"Cannot read messages: {ex.Message} ({ex.GetType()})");
                            break;
                        }

                        //Sleep 5 seconds between message checks
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        _logger.Error("Lost cellular network connection");
                        break;
                    }
                }
            }
            else
            {
                _logger.Fatal("No modem found");
            }

            //Close modem connection
            try
            {
                _comm.Close();
            }
            catch(Exception) { }
        }

        static bool TestConnection(string port)
        {
            _comm = new GsmCommMain(port, ModemBaud, ModemTimeout);
            try
            {
                _comm.Open();
                if(!_comm.IsConnected())
                {
                    _logger.Debug($"No modem on {port}");
                    _comm.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Test error: {ex.Message}");
                return false;
            }

            _logger.Debug($"Test succeeded on {port}");
            return true;
        }

        static void ChangePassword(string incomingphonenumber, string incomingtext)
        {
            string mailbody = $"Received request from: {incomingphonenumber}<br>SMS Text: {incomingtext}<br>Result: ";
            _logger.Info("Received SMS");
            _logger.Info($"Phone number: {incomingphonenumber}");
            _logger.Info($"Incoming text: {incomingtext}");

            //Check if this is a valid phone number. This check let us ignore messages from short/text numbers
            PhoneNumber phoneNumber = null;
            try
            {
                phoneNumber = _phoneNumberUtil.Parse(incomingphonenumber, _currentPhoneRegion);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception during phone number parsing:  {ex.Message} ({ex.GetType()})");
            }

            if (phoneNumber == null || !_phoneNumberUtil.IsValidNumber(phoneNumber) || incomingtext.Count(f => f == ' ') > 2)
            {
                _logger.Warn("Ignoring SMS, Reason: Invalid phone number");
                SendMail("SMS Self Service - Invalid SMS received", mailbody + "Invalid (Operator/Advertisement) SMS, ignoring...");
                return;
            }

            if (_config.GetValue("Protection/Bruteforce/Enabled",true) && _intrusionsList.ContainsKey(incomingphonenumber) && _intrusionsList[incomingphonenumber] > _config.GetValue("Protection/Bruteforce/MaximumRetries",5))
            {
                _logger.Warn("Ignoring SMS, Reason: possible account name bruteforcing.");
                return;
            }

            //Generate new password
            string newpass = Regex.Replace(Membership.GeneratePassword(8, 0), @"[^a-zA-Z0-9]", m => "9");

            try
            {
                //Get the username from various formats
                string login = incomingtext.Replace("/", "\\").ToLower();
                if (login.Contains("@")) // username@domain.com
                    login = login.Split('@')[0];
                if (login.Contains("\\")) // domain.com\username
                    login = login.Split('\\')[1];
                login = login.Trim();

                PasswordChangeResult result = _activeDirectory.ChangeUserPassword(login, newpass, phoneNumber.NationalNumber.ToString());
                string message;
                if (result == PasswordChangeResult.Success)
                {
                    _logger.Info($"Successfully changed password for {login}");

                    message = _config.GetValue("Messages/Success", "Your temporary password:");
                    if (!string.IsNullOrEmpty(message))
                        message += " " + newpass;

                    mailbody += "Successfully changed password";
                }
                else if (result == PasswordChangeResult.UserNotFound)
                {
                    _logger.Error($"User \"{login}\" not found");

                    message = _config.GetValue("Messages/UserNotFound", "Incorrect username");

                    mailbody += "User not found";
                }
                else if (result == PasswordChangeResult.DisabledAccount)
                {
                    _logger.Error("Account is disabled: " + login);

                    message = _config.GetValue("Messages/AccountDisabled",
                        "Account for this user is currently disabled");

                    mailbody += "Account is disabled";
                }
                else if (result == PasswordChangeResult.NoFingerprintAttached)
                {
                    _logger.Error($"No phone number attached to account: {login}");

                    message = _config.GetValue("Messages/NoPhoneAttached",
                        "This service cannot be used by this user");

                    mailbody += "No phone number attached";
                }
                else if (result == PasswordChangeResult.InvalidFingerprint)
                {
                    _logger.Warn($"INTRUSION? PHONE NUMBER DIFFERS FROM ONE ASSOCIATED WITH THIS ACCOUNT! Incoming number: {incomingphonenumber}, incoming text: {incomingtext}");

                    message = _config.GetValue("Messages/IncorrectNumber", "");

                    mailbody += "<font color=\"red\">This phone number is not the one associated with this account.</font>";

                    if (!_intrusionsList.ContainsKey(incomingphonenumber))
                        _intrusionsList[incomingphonenumber] = 0;
                    _intrusionsList[incomingphonenumber] += 1;
                }
                else
                {
                    _logger.Fatal($"Error while when fulfilling password change request: {result}");

                    message = _config.GetValue("Messages/InternalError", "Service temporary not available");

                    mailbody += $"<font color=\"red\">ERROR WHILE FULFILLING REQUEST: {result}</font>";
                }

                if (!string.IsNullOrEmpty(message))
                    Utils.SendMessage(_comm, incomingphonenumber, message);

                SendMail("SMS Self Service - Password Change Request", mailbody);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Error while changing user password: {ex.Message} ({ex.GetType()}) | {ex}");
            }
        }

        static void SendMail(string subject, string body)
        {
            if(_config.GetValue("Mail/Enabled", false))
                _mailHelper.SendMail(_config.GetValue("Mail/Target", ""), subject, body);
        }

        static void infotimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool modemstatus = _comm.IsOpen() && _comm.IsConnected();
            string modeminfo = "";
            try
            {
                if (modemstatus)
                    modeminfo = $"\r\nPort: {_comm.PortName}\r\nOperator: {_comm.GetCurrentOperator().TheOperator}" +
                                $"\r\nSignal: { (_comm.GetSignalQuality().SignalStrength == 99 ? "Unknown" : (Math.Floor(((double)_comm.GetSignalQuality().SignalStrength / 31) * 100)) + "%")}\r\n";
            }
            catch (Exception)
            {
                modemstatus = false;
            }
            _logger.Info($"Application Information:\r\nModem status: {(modemstatus ? "OK" : "ERROR") + modeminfo}");
        }

        static void comm_LoglineAdded(object sender, LoglineAddedEventArgs e)
        {
            _logger.Debug($"MODEM DEBUG:\r\nLEVEL: {e.Level}\r\nTEXT: {e.Text}");
        }
    }
}