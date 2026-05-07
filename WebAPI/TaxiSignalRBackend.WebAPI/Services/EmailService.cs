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
            var host     = _config["Email:Host"]     ?? "smtp.gmail.com";
            var port     = int.Parse(_config["Email:Port"] ?? "587");
            var username = _config["Email:Username"] ?? throw new Exception("Email:Username ayarlanmamış");
            // Şifredeki boşlukları temizle (Google App Password boşluklu girilebilir)
            var password = (_config["Email:Password"] ?? throw new Exception("Email:Password ayarlanmamış")).Replace(" ", "");

            Console.WriteLine($"📧 SMTP bağlantısı → {host}:{port} ({username})");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Erzurum BB App", username));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = "Doğrulama Kodunuz";
            message.Body = new TextPart("html") { Text = BuildHtml(code) };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            Console.WriteLine("✅ SMTP bağlantısı kuruldu");

            await client.AuthenticateAsync(username, password);
            Console.WriteLine("✅ SMTP kimlik doğrulandı");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            Console.WriteLine($"✅ Email gönderildi: {toEmail}");
        }

        private static string BuildHtml(string code) => $@"
            <div style='font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:24px;background:#f5f5f5;border-radius:12px;'>
              <h2 style='color:#1A237E;'>Hesabınızı Doğrulayın</h2>
              <p>Doğrulama kodunuz:</p>
              <div style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#0D47A1;padding:16px;background:#fff;border-radius:8px;text-align:center;'>{code}</div>
              <p style='color:#666;font-size:12px;margin-top:16px;'>Bu kodu kimseyle paylaşmayın. 15 dakika geçerlidir.</p>
            </div>";
    }
}
