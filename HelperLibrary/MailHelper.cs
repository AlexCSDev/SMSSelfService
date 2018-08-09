using System;
using System.Net;
using System.Net.Mail;

namespace HelperLibrary
{
    public class MailHelper
    {
        private readonly string _username;
        private readonly string _host;
        private readonly string _password;
        private readonly bool _useSSL;

        public MailHelper(string host, bool ssl, string username, string password)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host), "Host cannot be null");
            _useSSL = ssl;
            _username = username ?? throw new ArgumentNullException(nameof(username), "Username cannot be null"); ;
            _password = password ?? throw new ArgumentNullException(nameof(password), "Password cannot be null"); ;
        }

        public void SendMail(string targetEmail, string subject, string body)
        {
            if (targetEmail == null) throw new ArgumentNullException(nameof(targetEmail));

            MailMessage mail = new MailMessage(_username, targetEmail);
            mail.Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            mail.Body = body ?? throw new ArgumentNullException(nameof(body));
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient(_host);
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = _useSSL;
            if (!string.IsNullOrEmpty(_password))
                client.Credentials = new NetworkCredential(_username, _password);

            client.Send(mail);
        }
    }
}
