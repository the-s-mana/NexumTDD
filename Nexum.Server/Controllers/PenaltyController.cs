using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Services.Penalty;

namespace Nexum.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PenaltyController : ControllerBase
    {
        private readonly Penalty _penaltyService;

        //public readonly IDailyPenaltyStrategy dailyPenaltyStrategy;

        public PenaltyController(Penalty penaltyService)
        {
            _penaltyService = penaltyService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePenalty([FromBody] PenaltyRequest request)
        {
            try
            {
                var result = await _penaltyService.GetPenalty(request); // ใช้ await
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
