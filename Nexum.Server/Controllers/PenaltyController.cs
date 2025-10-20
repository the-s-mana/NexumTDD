using System.Threading.Tasks;
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
        [HttpGet("GetAllPenaltyPolicies")]
        public async Task<List<PenaltyPolicy>> GetAllPenaltyPolicies()
        {
            return await penalty.GetAllPolicies();
        }
        [HttpPost("CreatePenaltyPolicies")]
        public async Task<PenaltyPolicy> CreatePenaltyPolicies(PenaltyPolicy productContact)
        {
            return await penalty.CreatePolicies(productContact);
        }


    }
}
