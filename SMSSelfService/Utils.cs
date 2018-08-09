using System;
using System.IO;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using NLog;

namespace SMSSelfService
{
    public static class Utils
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void SendMessage(GsmCommMain comm, string phonenumber, string text)
        {
            try
            {
                _logger.Info($"Sending SMS to {phonenumber}");
                byte dcs = (byte)DataCodingScheme.GeneralCoding.Alpha16Bit;
                SmsSubmitPdu pdu = new SmsSubmitPdu(text, phonenumber, dcs);
                comm.SendMessage(pdu);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Error while sending SMS to {phonenumber}: {ex.Message} ({ex.GetType()})");
            }
        }

        public static string ApplicationLocationPath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}
