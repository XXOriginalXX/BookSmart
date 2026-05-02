using System.Text;
using System.Text.Json;
using AppointmentSystem.API.Models;

namespace AppointmentSystem.API.Services
{
    public class PredictionService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public PredictionService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["Prediction:BaseUrl"] ?? "http://localhost:8000";
        }

        public async Task<PredictionResult> PredictAsync(PredictionRequest req)
        {
            var payload = JsonSerializer.Serialize(req, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{_baseUrl}/predict", content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Prediction service error: {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PredictionResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? throw new Exception("Empty response from prediction service.");
        }
    }
}