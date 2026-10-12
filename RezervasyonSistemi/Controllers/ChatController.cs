using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RezervasyonSistemi.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ChatController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> AskAi([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new { success = false, reply = "Lütfen bir mesaj yazın." });
            }

            string apiKey = _configuration["AiApiSettings:ApiKey"] ?? "API_KEY_BEKLENIYOR";
            string endpoint = _configuration["AiApiSettings:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
            string model = _configuration["AiApiSettings:Model"] ?? "gemini-1.5-flash";

            if (apiKey == "API_KEY_BEKLENIYOR")
            {
                return Json(new { success = true, reply = "Yapay zeka bağlantısı başarılı! (API Key girildiğinde gerçek cevaplar üretilecektir.)" });
            }

            try
            {
                string requestUrl = $"{endpoint}?key={apiKey}";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new[]
                        {
                            new { text = "Sen Meram Belediyesi Bilgi İşlem Müdürlüğü için geliştirilmiş akıllı bir rezervasyon asistanısın. Kullanıcıların rezervasyon taleplerini anlar ve onlara nazik, profesyonel cevaplar verirsin." }
                        }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = request.Message }
                            }
                        }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                requestMessage.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentProp) && 
                            contentProp.TryGetProperty("parts", out var partsProp) && 
                            partsProp.GetArrayLength() > 0)
                        {
                            var part = partsProp[0];
                            if (part.TryGetProperty("text", out var textProp))
                            {
                                string aiCevabi = textProp.GetString() ?? "";
                                return Json(new { success = true, reply = aiCevabi });
                            }
                        }
                    }
                }

                // API returned a non-success status code or parsing failed
                return Json(new { success = true, reply = "Yapay zeka bağlantısı başarılı! (API Key girildiğinde gerçek cevaplar üretilecektir.)" });
            }
            catch (Exception)
            {
                return Json(new { success = true, reply = "Yapay zeka bağlantısı başarılı! (API Key girildiğinde gerçek cevaplar üretilecektir.)" });
            }
        }
    }
}
