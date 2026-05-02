using AppointmentSystem.API.Models;
using AppointmentSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.API.Controllers
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

        [HttpPost("noshow")]
        public async Task<IActionResult> Predict([FromBody] PredictionRequest request)
        {
            try
            {
                var result = await _predictionService.PredictAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}