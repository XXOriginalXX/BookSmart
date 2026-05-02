using BookSmart.API.Models;
using BookSmart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookSmart.API.Controllers
{
    [ApiController]
    [Route("api/prediction")]
    public class PredictionController : ControllerBase
    {
        private readonly PredictionService _predictionService;

        public PredictionController(PredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] PredictionRequest request)
        {
            var result = await _predictionService.PredictAsync(request);
            
            if (result == null)
                return StatusCode(500, new { message = "AI Analysis failed." });

            return Ok(result);
        }
    }
}