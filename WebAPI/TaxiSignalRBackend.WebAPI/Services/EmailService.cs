using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TaxiSignalRBackend.WebAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private static readonly HttpClient _http = new HttpClient();

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendVerificationEmail(string toEmail, string code)
        {
            var sendGridKey = _config["SendGrid:ApiKey"];
            var resendKey   = _config["Resend:ApiKey"];

            if (!string.IsNullOrEmpty(sendGridKey))
            {
                await SendViaSendGrid(toEmail, code, sendGridKey);
            }
            else if (!string.IsNullOrEmpty(resendKey) && resendKey != "YOUR_RESEND_API_KEY")
            {
                await SendViaResend(toEmail, code, resendKey);
            }
            else
            {
                await SendViaSmtp(toEmail, code);
            }
        }

        private async Task SendViaResend(string toEmail, string code, string apiKey)
        {
            var payload = new
            {
                from = "Erzurum BB App <noreply@resend.dev>",
                to = new[] { toEmail },
                subject = "Doğrulama Kodunuz",
                html = BuildHtml(code)
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Resend API hatası: {response.StatusCode} — {body}");

            Console.WriteLine($"✅ Resend ile email gönderildi: {toEmail}");
        }

        private async Task SendViaSendGrid(string toEmail, string code, string apiKey)
        {
            var payload = new
            {
                personalizations = new[]
                {
                    new { to = new[] { new { email = toEmail } } }
                },
                from = new { email = _config["SendGrid:FromEmail"] ?? _config["Email:Username"] ?? "erzbbappetu@gmail.com", name = "Erzurum BB App" },
                subject = "Doğrulama Kodunuz",
                content = new[]
                {
                    new { type = "text/html", value = BuildHtml(code) }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"SendGrid hatası: {response.StatusCode} — {body}");
            }

            Console.WriteLine($"✅ SendGrid ile email gönderildi: {toEmail}");
        }

        private static string BuildHtml(string code) => $@"
            <div style='font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:24px;background:#f5f5f5;border-radius:12px;'>
              <h2 style='color:#1A237E;'>Hesabınızı Doğrulayın</h2>
              <p>Doğrulama kodunuz:</p>
              <div style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#0D47A1;padding:16px;background:#fff;border-radius:8px;text-align:center;'>{code}</div>
              <p style='color:#666;font-size:12px;margin-top:16px;'>Bu kodu kimseyle paylaşmayın. 15 dakika geçerlidir.</p>
            </div>";

        private async Task SendViaSmtp(string toEmail, string code)
        {
            // SMTP fallback (Railway'de çalışmayabilir)
            using var client = new System.Net.Mail.SmtpClient(_config["Email:Host"], int.Parse(_config["Email:Port"] ?? "587"))
            {
                Credentials = new System.Net.NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                EnableSsl = true,
                Timeout = 10000
            };

            var mail = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_config["Email:Username"]!, "Erzurum BB App"),
                Subject = "Doğrulama Kodunuz",
                Body = $"<h2>Doğrulama Kodunuz: <strong>{code}</strong></h2>",
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
            Console.WriteLine($"✅ SMTP ile email gönderildi: {toEmail}");
        }
    }
}
