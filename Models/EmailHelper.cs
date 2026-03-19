using System.Net;
using System.Net.Mail;

namespace WEBDULICH.Helpers
{
    public static class EmailHelper
    {
        public static void SendEmail(string to, string subject, string body)
        {
            var fromEmail = "yourgmail@gmail.com"; 
          // nếu như nó khác tên miền gmail.com thì nó xử lý khác cơ bản // 

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
               
                EnableSsl = true
            };

            var message = new MailMessage(fromEmail, to, subject, body)
            {
                IsBodyHtml = true
            };

            smtpClient.Send(message);
        }
    }
}
