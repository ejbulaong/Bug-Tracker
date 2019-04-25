using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Bug_Tracker.Models
{
    public class EmailService : IIdentityMessageService
    {
        //Getting the settings from our private.config setup in web.config
        private string SmtpHost = ConfigurationManager.
            AppSettings["SmtpHost"];
        private int SmtpPort = Convert.ToInt32(
            ConfigurationManager.AppSettings["SmtpPort"]);
        private string SmtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
        private string SmtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
        private string SmtpFrom = ConfigurationManager.AppSettings["SmtpFrom"];

        public void Send(List<string> to, string body, string subject)
        {
            //Creates a MailMessage required to send messages
            var message = new MailMessage();
            foreach(var e in to)
            {
                message.To.Add(e);
            }
            message.From = new MailAddress(SmtpFrom);
            message.Body = body;
            message.Subject = subject;
            message.IsBodyHtml = true;

            //Creates a SmtpClient required to handle the communication
            //between our application and the SMTP Server
            var smtpClient = new SmtpClient(SmtpHost, SmtpPort);
            smtpClient.Credentials =
                new NetworkCredential(SmtpUsername, SmtpPassword);
            smtpClient.EnableSsl = true;

            //Send the message
            smtpClient.Send(message);
        }

        public Task SendAsync(IdentityMessage message)
        {
            var to = new List<string>();
            //Caling our sync method in a async way.
            return Task.Run(() =>
            Send(to,message.Body, message.Subject));
        }
    }
}