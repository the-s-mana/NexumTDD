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
        public readonly IPenalty  penalty;

        //public readonly IDailyPenaltyStrategy dailyPenaltyStrategy;

        public PenaltyController(IPenalty penalty)
        {
            this.penalty = penalty;
        }

        [HttpPost("CalculatePenalty")]
        public PenaltyResponse CalculatePenalty(PenaltyRequest penaltyRequest)
        {
            return penalty.GetPenalty(penaltyRequest);
        }



    }
}
