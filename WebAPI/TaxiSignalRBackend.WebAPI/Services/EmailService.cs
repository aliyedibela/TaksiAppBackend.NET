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
            var password = _config["Email:Password"] ?? throw new Exception("Email:Password ayarlanmamış");

            Console.WriteLine($"📧 Gmail SMTP ile email gönderiliyor → {toEmail}");

            using var client = new System.Net.Mail.SmtpClient(host, port)
            {
                Credentials = new System.Net.NetworkCredential(username, password),
                EnableSsl   = true,
                Timeout     = 15000
            };

            var mail = new System.Net.Mail.MailMessage
            {
                From       = new System.Net.Mail.MailAddress(username, "Erzurum BB App"),
                Subject    = "Doğrulama Kodunuz",
                Body       = BuildHtml(code),
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
            Console.WriteLine($"✅ Gmail SMTP ile email gönderildi: {toEmail}");
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
