using Microsoft.AspNetCore.Mvc;
using Nexum.Server.API.Dto;
using Nexum.Server.DAC;

namespace Nexum.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IssuerController : ControllerBase
    {
        private readonly IIssuersDAC _issuersDAC;
        public IssuerController(IIssuersDAC dac) => _issuersDAC = dac;

        [HttpGet("issuers")]
        public async Task<ActionResult<List<IssuerResponseDTO>>> GetAllIssuers()
            => Ok(await _issuersDAC.GetAllAsync());

        [HttpGet("issuers/{id:int}")]
        public async Task<ActionResult<IssuerResponseDTO>> GetIssuerById(int id)
        {
            var rec = await _issuersDAC.GetByIdAsync(id);
            if (rec == null) return NotFound(new { message = $"Issuer {id} not found." });
            return Ok(rec);
        }

        [HttpPost("issuer")]
        public async Task<ActionResult<IssuerResponseDTO>> UpsertIssuer([FromBody] IssuerRequestDTO body)
        {
            var upserted = await _issuersDAC.UpsertAsync(body);
            return Ok(upserted);
        }

        [HttpPost("issuer/{issuerId:int}/offers/{policyId:int}")]
        public async Task<ActionResult> RelateIssuerToPolicy(int issuerId, int policyId)
        {
            await _issuersDAC.RelateIssuerToPolicyAsync(issuerId, policyId);
            return NoContent();
        }

        [HttpGet("issuer/{issuerId:int}/policies")]
        public async Task<ActionResult<List<PenaltyPolicyResponseDTO>>> GetPoliciesByIssuer(int issuerId)
            => Ok(await _issuersDAC.GetPoliciesByIssuerAsync(issuerId));
    }
}
