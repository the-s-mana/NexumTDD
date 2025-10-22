using Microsoft.AspNetCore.Mvc;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Models.Penalty.Surreal;
using Nexum.Server.Services.Penalty;

namespace Nexum.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PenaltyController : ControllerBase
    {
        public readonly IPenalty  penalty;
        private readonly IPenaltyPoliciesDAC _penaltyPoliciesDAC;

        //public readonly IDailyPenaltyStrategy dailyPenaltyStrategy;

        public PenaltyController(IPenalty penalty, IPenaltyPoliciesDAC penaltyPoliciesDAC)
        {
            this.penalty = penalty;
            this._penaltyPoliciesDAC = penaltyPoliciesDAC;
        }

        [HttpGet("policies")]
        public async Task<ActionResult<List<PenaltyPolicyRecord>>> GetAllPenaltyPolicies()
        {
            var list = await _penaltyPoliciesDAC.GetAllPenaltyPoliciesAsync();
            return Ok(list);
        }

        // GET /Penalty/policies/{id}
        // คืน policy ตาม PenaltyPolicyID
        [HttpGet("policies/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyRecord>> GetPolicyById(int id)
        {
            var rec = await _penaltyPoliciesDAC.GetPenaltyPolicyByIdAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy {id} not found." });
            return Ok(rec);
        }

        // GET /Penalty/policies/record/{id}
        [HttpGet("policies/record/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyRecord>> GetPolicyByRecordId(int id)
        {
            var rec = await _penaltyPoliciesDAC.GetPenaltyPolicyByRecordIdAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy record {id} not found." });
            return Ok(rec);
        }

        //// POST /Penalty/policy
        //[HttpPost("policy")]
        //public async Task<ActionResult<PenaltyPolicyRecord>> CreatePolicy(PenaltyPolicyRecord body)
        //{
        //    var created = await _penaltyPoliciesDAC.CreatePolicyAsync(body);
        //    return CreatedAtAction(nameof(GetPolicyById), new { id = created.PenaltyPolicyID }, created);
        //}

        // GET /Penalty/query/policies
        [HttpGet("query/policies")]
        public async Task<ActionResult<List<PenaltyPolicyRecord>>> QueryAllPenaltyPolicies()
        {
            var rec = await _penaltyPoliciesDAC.Query_AllAsync();
            if (rec == null) return NotFound(new { message = $"Policy not found." });
            return Ok(rec);
        }

        // GET /Penalty/query/policies/{id}
        [HttpGet("query/policies/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyRecord>> QueryPolicyById(int id)
        {
            var rec = await _penaltyPoliciesDAC.Query_OneAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy {id} not found." });
            return Ok(rec);
        }

        // GET /Penalty/policiesX/{id}
        [HttpGet("policiesX/{id:int}")]
        public async Task<ActionResult<ProductContact>> GetPolicyByIdX(int id)
        {
            var rec = await _penaltyPoliciesDAC.GetPenaltyPolicyByIdXAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy {id} not found." });
            return Ok(rec);
        }

        //[HttpPost("CalculatePenalty")]
        //public PenaltyResponse CalculatePenalty(PenaltyRequest penaltyRequest)
        //{
        //    return penalty.GetPenalty(penaltyRequest);
        //}
    }
}
