using AWSServerlessFitDev.Util;
using System;
using System.Net.Mail;
using System.Text;

namespace AWSServerlessFitDev.Services
{
    public class EmailService : IEmailService
    {
        string DefaultSenderEmail;
        string SMTPServerName;
        int SMTPServerPort;
        string DefaultSenderEmailPassword;
        public EmailService()
        {
            DefaultSenderEmail = Constants.DefaultSenderEmail;
            SMTPServerName = Constants.SMTPServerName;
            SMTPServerPort = Constants.SMTPServerPort;
            DefaultSenderEmailPassword = Constants.DefaultSenderEmailPassword;
        }
        public void SendEmail(string to, string subject, string body)
        {
            MailMessage message = new MailMessage(DefaultSenderEmail, to);
            message.Subject = subject;
            message.Body = body;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient(SMTPServerName, SMTPServerPort); //Gmail smtp    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential(DefaultSenderEmail, DefaultSenderEmailPassword);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
