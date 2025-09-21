using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace AiChatBot.Services
{
    public class OpenAiChatService : IChatService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly Dictionary<string, string> _faqs;

        public OpenAiChatService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            // Load local FAQs
            var faqsPath = Path.Combine(AppContext.BaseDirectory, "faqs.json");
            if (File.Exists(faqsPath))
            {
                var text = File.ReadAllText(faqsPath);
                _faqs = JsonSerializer.Deserialize<Dictionary<string, string>>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new Dictionary<string, string>();
            }
            else
            {
                _faqs = new Dictionary<string, string>();
            }

            // Setup Authorization header from configuration (user secrets or env)
            var apiKey = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrEmpty(apiKey))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
        }

        public async Task<string> GetAnswerAsync(List<(string Role, string Message)> messages)
        {
            if (messages == null || messages.Count == 0)
                return "Please enter a question.";

            var lastUserInput = messages.LastOrDefault(m => m.Role == "user").Message;

            // Check FAQs first
            var key = lastUserInput.Trim().ToLowerInvariant();
            if (_faqs.TryGetValue(key, out var ans))
                return ans;

            foreach (var k in _faqs.Keys)
            {
                if (key.Contains(k.ToLowerInvariant()))
                    return _faqs[k];
            }

            // No local answer → fallback to OpenAI
            var apiKey = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                return "(No local match) I don't know that yet. Add it to faqs.json or add an API key to get AI answers.";
            }

            var model = _config["OpenAI:Model"] ?? "gpt-3.5-turbo";
            var payload = new
            {
                model = model,
                messages = messages.Select(m => new { role = m.Item1, content = m.Item2 }),
                max_tokens = 400
            };

            var resp = await _http.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload);

            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                return $"(AI Error {resp.StatusCode}) {text}";
            }

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentProp))
            {
                return contentProp.GetString() ?? "";
            }

            return "(AI responded with empty answer)";
        }

    }
}
