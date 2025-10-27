using Microsoft.AspNetCore.Mvc;
using Nexum.Server.API.Dto;
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
        public async Task<ActionResult<List<PenaltyPolicyResponseDTO>>> GetAllPenaltyPolicies()
        {
            var list = await _penaltyPoliciesDAC.GetAllPenaltyPoliciesAsync();
            return Ok(list);
        }

        // GET /Penalty/policies/{id}
        // คืน policy ตาม PenaltyPolicyID
        [HttpGet("policies/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> GetPolicyById(int id)
        {
            var rec = await _penaltyPoliciesDAC.GetPenaltyPolicyByIdAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy {id} not found." });
            return Ok(rec);
        }

        // GET /Penalty/policies/record/{id}
        [HttpGet("policies/record/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> GetPolicyByRecordId(int id)
        {
            var rec = await _penaltyPoliciesDAC.GetPenaltyPolicyByRecordIdAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy record {id} not found." });
            return Ok(rec);
        }

        // POST /Penalty/policy
        [HttpPost("policy")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> CreatePolicy(PenaltyPolicyRequestDTO body)
        {
            var created = await _penaltyPoliciesDAC.CreatePenaltyPolicyAsync(body);
            return Ok(created);
            //return CreatedAtAction(nameof(GetPolicyById), new { id = created.PenaltyPolicyID }, created);
        }

        // DELETE /Penalty/policy/{id}
        [HttpDelete("policy/{id:int}")]
        public async Task<IActionResult> DeletePolicyById(int id)
        {
            await _penaltyPoliciesDAC.DeletePenaltyPolicyByIdAsync(id);
            return NoContent();
        }

        // DELETE /Penalty/policy/record/{id}
        [HttpDelete("policy/record/{id:int}")]
        public async Task<IActionResult> DeletePolicyByRecordId(int id)
        {
            await _penaltyPoliciesDAC.DeletePenaltyPolicyByRecordIdAsync(id);
            return NoContent();
        }

        // PUT /Penalty/policy
        [HttpPut("policy")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> UpsertPolicy(PenaltyPolicyRequestDTO body)
        {
            var upsert = await _penaltyPoliciesDAC.UpsertPenaltyPolicyAsync(body);
            return Ok(upsert);
        }

        // PATCH /Penalty/policy
        [HttpPatch("policy")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> UpdatePolicy(PenaltyPolicyRequestDTO body)
        {
            var update = await _penaltyPoliciesDAC.UpdatePenaltyPolicyAsync(body);
            return Ok(update);
        }

        // GET /Penalty/policies/query
        [HttpGet("policies/query")]
        public async Task<ActionResult<List<PenaltyPolicyResponseDTO>>> QueryAllPenaltyPolicies()
        {
            var rec = await _penaltyPoliciesDAC.Query_AllAsync();
            if (rec == null) return NotFound(new { message = $"Policy not found." });
            return Ok(rec);
        }

        // GET /Penalty/policies/query/{id}
        [HttpGet("policies/query/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> QueryPolicyById(int id)
        {
            var rec = await _penaltyPoliciesDAC.Query_OneAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy {id} not found." });
            return Ok(rec);
        }

        // GET /Penalty/policiesX/{id}
        [HttpGet("policiesX/{id:int}")]
        public async Task<ActionResult<PenaltyPolicyResponseDTO>> GetPolicyByIdX(int id)
        {
            var rec = await _penaltyPoliciesDAC.GetPenaltyPolicyByIdXAsync(id);
            if (rec == null) return NotFound(new { message = $"Policy {id} not found." });
            return Ok(rec);
        }

        [HttpPost("Penalty Calculate")]
        public async Task<ActionResult<PenaltyResponse>> PenaltyCalculate([FromBody] PenaltyRequest req)
        {
            var result = await penalty.GetPenaltyAsync(req);
            return Ok(result);
        }
    }
}
