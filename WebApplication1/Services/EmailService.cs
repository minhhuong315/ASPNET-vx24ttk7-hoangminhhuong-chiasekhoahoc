using System.Net;
using System.Net.Mail;

namespace OnlineLearningPlatform.Services
{
    public interface IEmailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlMessage);
    }

    public class SmtpEmailService
        : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlMessage)
        {
            var host =
                _configuration["Email:SmtpHost"];

            var username =
                _configuration["Email:Username"];

            var password =
                _configuration["Email:Password"];

            var fromEmail =
                _configuration["Email:FromEmail"];

            var fromName =
                _configuration["Email:FromName"]
                ?? "EduLearn";

            var port =
                _configuration.GetValue<int?>(
                    "Email:SmtpPort")
                ?? 587;

            var enableSsl =
                _configuration.GetValue<bool?>(
                    "Email:EnableSsl")
                ?? true;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình SMTP Email trong User Secrets.");
            }

            using var message =
                new MailMessage
                {
                    From =
                        new MailAddress(
                            fromEmail,
                            fromName),

                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

            message.To.Add(
                new MailAddress(toEmail));

            using var client =
                new SmtpClient(
                    host,
                    port)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,

                    Credentials =
                        new NetworkCredential(
                            username,
                            password)
                };

            await client.SendMailAsync(message);
        }
    }
}
