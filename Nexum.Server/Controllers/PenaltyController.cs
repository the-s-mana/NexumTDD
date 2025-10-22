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
        public readonly IPenaltyPolicies penaltyPolicies;

        //public readonly IDailyPenaltyStrategy dailyPenaltyStrategy;

        public PenaltyController(IPenalty penalty, IPenaltyPolicies penaltyPolicies)
        {
            this.penalty = penalty;
            this.penaltyPolicies = penaltyPolicies;
        }

        [HttpPost("CalculatePenalty")]
        public PenaltyResponse CalculatePenalty(PenaltyRequest penaltyRequest)
        {
            return penalty.GetPenalty(penaltyRequest);
        }




        #region PenaltyPolicies
        [HttpGet("GetAllPenaltyPolicies")]
        public async Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicies()
        {
            return await penaltyPolicies.GetAllPenaltyPolicies();
        }
        [HttpGet("GetPenaltyPolicyById/{id}")]
        public  async Task<PenaltyPolicyDTO> GetPenaltyPolicyById(string id)
        {
            return await penaltyPolicies.GetPenaltyPolicyByIdAsync(id);
        }
        [HttpPost("CreatePenaltyPolicy")]
        public async Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPolicies.CreatePenaltyPolicyAsync(penaltyPolicyDto);
        }
        [HttpPut("UpdatePenaltyPolicy")]
        public async Task<PenaltyPolicyDTO> UpdatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPolicies.UpdatePenaltyPolicyAsync(penaltyPolicyDto);
        }
        [HttpPost("UpsertPenaltyPolicy")]
        public async Task<PenaltyPolicyDTO> UpsertPenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPolicies.UpsertPenaltyPolicyAsync(penaltyPolicyDto);
        }
        [HttpDelete("DeletePenaltyPolicy/{id}")]
        public async Task<bool> DeletePenaltyPolicyAsync(string id)
        {
            return await penaltyPolicies.DeletePenaltyPolicyAsync(id);
        }
        #endregion







    }
}
