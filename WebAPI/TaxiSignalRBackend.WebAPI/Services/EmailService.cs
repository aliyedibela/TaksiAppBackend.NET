using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TaxiSignalRBackend.WebAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendVerificationEmail(string toEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Erzurum BB App", _config["Email:Username"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Doğrulama Kodunuz";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <h2>Hesabınızı Doğrulama</h2>
                    <p>Doğrulama kodunuz: <strong>{code}</strong></p>
                    <p>Bu kodu uygulamaya girerek hesabınızı aktif edin.</p>
                "
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}