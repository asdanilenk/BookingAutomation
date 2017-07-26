using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BookingCommon
{
    public class EmailClient
    {
        public static void SendEmail(String body)
        {
            String username = ConfigurationManager.AppSettings["emailSender"];
            String password = ConfigurationManager.AppSettings["emailSenderPassword"];
            String to = ConfigurationManager.AppSettings["emailReceiver"];
            String subject = ConfigurationManager.AppSettings["emailSubject"];

            new SmtpClient
            {
                Host = "Smtp.Gmail.com",
                Port = 587,
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            }.Send(new MailMessage
            {
                From = new MailAddress(username, "Booking automation"),
                To = { to },
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8
            });
        }
    }
}
