using BookSmart.API.Models;
using System.Diagnostics;
using System.Text.Json;

namespace BookSmart.API.Services
{
    public class PredictionService
    {
        private readonly string _pythonPath = "python";
        private readonly string _scriptPath = "predict_bridge.py";

        public async Task<PredictionResponse> PredictAsync(PredictionRequest request)
        {
            var payload = JsonSerializer.Serialize(request);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = $"{_scriptPath} \"{payload.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            using var reader = process.StandardOutput;
            string result = await reader.ReadToEndAsync();
            
            return JsonSerializer.Deserialize<PredictionResponse>(result) 
                   ?? new PredictionResponse();
        }
    }
}