using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TaxiSignalRBackend.WebAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private static readonly HttpClient _http = new();

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendVerificationEmail(string toEmail, string code)
        {
            // Önce düz env var adından oku (Railway uyumlu), yoksa nested config'e bak
            var clientId     = Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID")
                            ?? _config["Gmail:ClientId"]
                            ?? throw new Exception("GMAIL_CLIENT_ID eksik");
            var clientSecret = Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET")
                            ?? _config["Gmail:ClientSecret"]
                            ?? throw new Exception("GMAIL_CLIENT_SECRET eksik");
            var refreshToken = Environment.GetEnvironmentVariable("GMAIL_REFRESH_TOKEN")
                            ?? _config["Gmail:RefreshToken"]
                            ?? throw new Exception("GMAIL_REFRESH_TOKEN eksik");
            var fromEmail    = Environment.GetEnvironmentVariable("GMAIL_FROM_EMAIL")
                            ?? _config["Gmail:FromEmail"]
                            ?? "erzurumbbappetu@gmail.com";

            Console.WriteLine($"📧 Gmail API ile email gönderiliyor → {toEmail}");

            // 1. Refresh token ile access token al
            var tokenResp = await _http.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"]     = clientId,
                    ["client_secret"] = clientSecret,
                    ["refresh_token"] = refreshToken,
                    ["grant_type"]    = "refresh_token"
                }));

            var tokenJson = await tokenResp.Content.ReadAsStringAsync();
            if (!tokenResp.IsSuccessStatusCode)
                throw new Exception($"Token alınamadı: {tokenJson}");

            var tokenDoc    = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString()
                              ?? throw new Exception("access_token boş geldi");

            Console.WriteLine("✅ Access token alındı");

            // 2. RFC 2822 email mesajı oluştur
            var subject   = "=?UTF-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes("Doğrulama Kodunuz")) + "?=";
            var htmlBody  = BuildHtml(code);
            var rawEmail  = $"From: Erzurum BB App <{fromEmail}>\r\n" +
                            $"To: {toEmail}\r\n" +
                            $"Subject: {subject}\r\n" +
                            $"MIME-Version: 1.0\r\n" +
                            $"Content-Type: text/html; charset=UTF-8\r\n\r\n" +
                            htmlBody;

            // Base64url encode (Gmail API gereksinimi)
            var base64Url = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawEmail))
                            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

            // 3. Gmail API ile gönder
            var requestBody = JsonSerializer.Serialize(new { raw = base64Url });
            var request     = new HttpRequestMessage(HttpMethod.Post,
                "https://gmail.googleapis.com/gmail/v1/users/me/messages/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var sendResp = await _http.SendAsync(request);
            var sendJson = await sendResp.Content.ReadAsStringAsync();

            if (!sendResp.IsSuccessStatusCode)
                throw new Exception($"Email gönderilemedi: {sendJson}");

            Console.WriteLine($"✅ Gmail API ile email gönderildi: {toEmail}");
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
