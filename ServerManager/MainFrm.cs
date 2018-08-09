using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration.Install;
using System.IO;
using System.Security.Principal;
using System.Threading;
using ADLibrary;
using Bia.Countries.Iso3166;
using HelperLibrary;
using XMLConfig;

namespace ServerManager
{
    public partial class MainFrm : Form
    {
        private Config _config;
        private EncryptionHelper _encryptionHelper;
        public MainFrm()
        {
            InitializeComponent();
            _config = new Config($"{Application.StartupPath}\\config.xml", true);

            //Initialize encryption
            string entropy = _config.GetValue("Encryption/Entropy", "");
            bool newEntropy = false;
            if (string.IsNullOrEmpty(entropy))
            {
                entropy = null;
                newEntropy = true;
            }
            _encryptionHelper = new EncryptionHelper(entropy);

            if (newEntropy)
                _config.SetValue("Encryption/Entropy", _encryptionHelper.Entropy);
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            StatusChk_timer_Tick(null, null);

            phoneCountry_combobox.Items.AddRange(Countries.GetAllShortNames().ToArray());

            LoadSettings();
        }

        private void LoadSettings()
        {
            logdir_txtbox.Text = _config.GetValue("General/LogsDirectory", $"{Application.StartupPath}\\logs");
            smscenterphone_txtbox.Text = _config.GetValue("General/SMSCenter", "");
     
            dcaddress_txtbox.Text = _config.GetValue("Domain/ControllerAddress", "");
            dccontainer_txtbox.Text = _config.GetValue("Domain/Container", "");
            useadminaccount_chkbox.Checked = _config.GetValue("Domain/UseAdminAccount", false);
            adminusername_txtbox.Text = _config.GetValue("Domain/AdminUsername", "");
            adminpassword_txtbox.Text = _config.GetValue("Domain/AdminPassword", "");

            /*string encyptedAdminPassword = _config.GetValue("Domain/AdminPassword", "");

            if (!string.IsNullOrEmpty(encyptedAdminPassword))
            {
                try
                {
                    adminpassword_txtbox.Text = _encryptionHelper.Decrypt(encyptedAdminPassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Encryption error: unable to decrypt admin password.\nPossible reasons: configuration file belongs to another computer or you have tried to manually edit configuration file",
                        "SMS Self Service Server Manager - Error");
                }
            }*/

            enableemail_chkbox.Checked = _config.GetValue("Mail/Enabled", false);
            mailserver_txtbox.Text = _config.GetValue("Mail/Host", "");
            ssl_chkbox.Checked = _config.GetValue("Mail/UseSSL", false);
            mailuser_txtbox.Text = _config.GetValue("Mail/Username", "");
            mailpassword_txtbox.Text = _config.GetValue("Mail/Password", "");
            mailtarget_txtbox.Text = _config.GetValue("Mail/Target", "");

            successfulpasschangemsg_txtbox.Text = _config.GetValue("Messages/Success",
                "Your temporary password:");
            usernotfoundmsg_txtbox.Text = _config.GetValue("Messages/UserNotFound", "Incorrect username");
            accisdisabledmsg_txtbox.Text = _config.GetValue("Messages/AccountDisabled",
                "Account for this user is currently disabled");
            nophoneattachedmsg_txtbox.Text = _config.GetValue("Messages/NoPhoneAttached",
                "This service cannot be used by this user");
            incorrectnumbermsg_txtbox.Text = _config.GetValue("Messages/IncorrectNumber", "");
            internalerrormsg_txtbox.Text = _config.GetValue("Messages/InternalError", "Service temporary not available");

            bruteforceproten_chkbox.Checked = _config.GetValue("Protection/Bruteforce/Enabled", true);
            bruteforcemaxretries_numupdown.Text = _config.GetValue("Protection/Bruteforce/MaxRetries", "5");

            Country phoneCountry = Countries.GetCountryByAlpha2(_config.GetValue("Protection/Spam/PhoneRegion", "ZZ"));
            if (phoneCountry != null)
                phoneCountry_combobox.SelectedItem = phoneCountry.ShortName;

            eninfomsg_chkbox.Checked = _config.GetValue("General/InformationTimer/Enabled", false);
            infomsginterval_numupdown.Text = _config.GetValue("General/InformationTimer/Interval", "60");
            enatcmds_chkbox.Checked = _config.GetValue("Debug/ModemCommands", false);

            Main_tabctl.TabPages.Remove(ServerConsole_tabpage); //Not finished

            if (!File.Exists($"{Application.StartupPath}\\SMSSelfService.exe"))
            {
                Main_tabctl.TabPages.Remove(ServerControls_tabpage);
                //Main_tabctl.TabPages.Remove(ServerConsole_tabpage);
                Settings_tabctl.TabPages.Remove(protectionsettings_tabpage);
                Settings_tabctl.TabPages.Remove(notificationsettings_tabpage);
                Settings_tabctl.TabPages.Remove(GeneralSettings_tabpage);
                Settings_tabctl.TabPages.Remove(smsresponsesettings_tabpage);
                Settings_tabctl.TabPages.Remove(devsettings_tabpage);
            }
        }

        private void SaveSettings()
        {
            _config.SetValue("General/LogsDirectory", logdir_txtbox.Text);
            _config.SetValue("General/SMSCenter", smscenterphone_txtbox.Text);

            _config.SetValue("Domain/ControllerAddress", dcaddress_txtbox.Text);
            _config.SetValue("Domain/Container", dccontainer_txtbox.Text);
            _config.SetValue("Domain/UseAdminAccount", useadminaccount_chkbox.Checked.ToString());
            _config.SetValue("Domain/AdminUsername", adminusername_txtbox.Text);

            if (_config.GetValue("Domain/AdminPassword", "") != adminpassword_txtbox.Text)
            {
                string passwordValue;

                if (!string.IsNullOrEmpty(adminpassword_txtbox.Text))
                    passwordValue = _encryptionHelper.Encrypt(adminpassword_txtbox.Text);
                else
                    passwordValue = "";

                _config.SetValue("Domain/AdminPassword", passwordValue);
                adminpassword_txtbox.Text = passwordValue;
            }

            _config.SetValue("Mail/Enabled", enableemail_chkbox.Checked.ToString());
            _config.SetValue("Mail/Host", mailserver_txtbox.Text);
            _config.SetValue("Mail/UseSSL", ssl_chkbox.Checked.ToString());
            _config.SetValue("Mail/Username", mailuser_txtbox.Text);
            _config.SetValue("Mail/Password", mailpassword_txtbox.Text);
            _config.SetValue("Mail/Target", mailtarget_txtbox.Text);

            _config.SetValue("Messages/Success", successfulpasschangemsg_txtbox.Text);
            _config.SetValue("Messages/UserNotFound", usernotfoundmsg_txtbox.Text);
            _config.SetValue("Messages/AccountDisabled", accisdisabledmsg_txtbox.Text);
            _config.SetValue("Messages/NoPhoneAttached", nophoneattachedmsg_txtbox.Text);
            _config.SetValue("Messages,IncorrectNumber", incorrectnumbermsg_txtbox.Text);
            _config.SetValue("Messages/InternalError", internalerrormsg_txtbox.Text);

            _config.SetValue("Protection/Bruteforce/Enabled", bruteforceproten_chkbox.Checked.ToString());
            _config.SetValue("Protection/Bruteforce/MainRetries", bruteforcemaxretries_numupdown.Text);

            Country phoneCountry = Countries.GetCountryByShortName((string)phoneCountry_combobox.SelectedItem);
            if (phoneCountry != null)
                _config.SetValue("Protection/Spam/PhoneRegion", phoneCountry.Alpha2);

            _config.SetValue("General/InformationTimer/Enabled", eninfomsg_chkbox.Checked.ToString());
            _config.SetValue("General/InformationTimer/Interval", infomsginterval_numupdown.Text);
            _config.SetValue("Debug/ModemCommands", enatcmds_chkbox.Checked.ToString());
        }

        private ServiceController GetService()
        {
            ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "smsselfservice");
            return ctl;
        }

        private ServiceStatus GetServiceStatus()
        {
            ServiceController ctl = GetService();
            if (ctl == null)
                return ServiceStatus.NotInstalled;
            else
            {
                switch (ctl.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        return ServiceStatus.Stopped;
                    case ServiceControllerStatus.Running:
                        return ServiceStatus.Running;
                    default:
                        return ServiceStatus.Uknown;
                }
            }
        }

        public static void RunProcess(string path, string arguments)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = arguments;
            start.FileName = path;
            start.WorkingDirectory = Application.StartupPath;
            Process.Start(start);
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            if (GetServiceStatus() == ServiceStatus.NotInstalled)
                RunProcess($"{Application.StartupPath}\\SMSSelfService.exe", "");
            else
                GetService().Start();
        }

        private void StatusChk_timer_Tick(object sender, EventArgs e)
        {
            bool running = false;
            if (GetServiceStatus() == ServiceStatus.NotInstalled)
            {
                Process[] pname = Process.GetProcessesByName("smsselfservice");
                if (pname.Length != 0)
                {
                    running = true;
                }
                service_btn.Text = "Install service";
            }
            else
            {
                running = GetServiceStatus() == ServiceStatus.Running;
                service_btn.Text = "Uninstall service";
            }
            if (!running)
            {
                start_btn.Enabled = true;
                restart_btn.Enabled = false;
                stop_btn.Enabled = false;
                service_btn.Enabled = true;
                servicestatus_lbl.Text = "Stopped";
                servicestatus_lbl.ForeColor = Color.Red;
            }
            else
            {
                start_btn.Enabled = false;
                restart_btn.Enabled = true;
                stop_btn.Enabled = true;
                service_btn.Enabled = false;
                servicestatus_lbl.Text = "Running";
                servicestatus_lbl.ForeColor = Color.DarkGreen;
            }
            if (!IsUserAdministrator())
            {
                if (GetServiceStatus() != ServiceStatus.NotInstalled)
                {
                    start_btn.Enabled = false;
                    restart_btn.Enabled = false;
                    stop_btn.Enabled = false;
                }
                service_btn.Enabled = false;
                controlsdisabled_label.Visible = true;
            }
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            if (GetServiceStatus() == ServiceStatus.NotInstalled)
            {
                Process[] processes = Process.GetProcessesByName("smsselfservice");
	            processes[0].Kill();
            }
            else
                GetService().Stop();
        }

        private void service_btn_Click(object sender, EventArgs e)
        {
            string[] args;
            if (GetServiceStatus() != ServiceStatus.NotInstalled)
            {
                args = new string[1];
                args[0] = "SMSSelfService.exe";
               //args[1] = "/LogFile=serviceinstall.log";
            }
            else
            {
                args = new string[2];
                args[0] = "/u";
                args[1] = "SMSSelfService.exe";
                //args[2] = "/LogFile=serviceinstall.log";
            }

            try
            {
                ManagedInstallerClass.InstallHelper(args);
                MessageBox.Show("Service Sucessfully (un)installed", "SMS Self Service Server Manager");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to install service. Reason: {ex.Message} ({ex.GetType()})","SMS Self Service Server Manager",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        public bool IsUserAdministrator()
        {
            bool isAdmin;
            WindowsIdentity user = null;
            try
            {
                user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            finally
            {
                user?.Dispose();
            }
            return isAdmin;
        }

        private void restart_btn_Click(object sender, EventArgs e)
        {
            stop_btn_Click(null, null);
            Thread.Sleep(3000);
            start_btn_Click(null,null);
        }

        private void applysettings_btn_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void cancelsettings_btn_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void logfolderselect_btn_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                logdir_txtbox.Text = folderBrowserDialog.SelectedPath;
        }

        private void useadminaccount_chkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (useadminaccount_chkbox.Checked)
            {
                adminusername_txtbox.Enabled = true;
                adminpassword_txtbox.Enabled = true;
            }
            else
            {
                adminusername_txtbox.Enabled = false;
                adminpassword_txtbox.Enabled = false;
            }
        }
    }
}
