using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace StackOverFlowClone.Services.Implementations
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IConfiguration _config;
        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _apiKey = _config["GeminiApiKey"];
        }

        public async Task<string> GetChatResponse(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"
            );

            request.Headers.Add("X-goog-api-key", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);

                var responseText = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return responseText ?? "No Response from Gemini";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Gemini Error: {response.StatusCode} - {error}";
            }
        }
    }
}
