using System.Net;
using System.Net.Mail;

namespace StackOverFlowClone.Services.Implementations
{
    public class UserSettingsService
    {
        private readonly IConfiguration _configuration;
        public UserSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(0, 1000000).ToString("D6");
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var fromEmail = _configuration["Email"];
                var fromPassword = _configuration["Pass"];

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, "MyApp"),
                    Subject = "Your OTP Code",
                    Body = $"Your One-Time Password (OTP) is: {otp}",
                    IsBodyHtml = false
                };

                message.To.Add(toEmail);

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromEmail, fromPassword)
                };

                await smtp.SendMailAsync(message);
            }
            catch (SmtpException ex)
            {
                // Log the error (you can use ILogger if available)
                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw; // let your middleware handle 500 response
            }
        }

    }
}
