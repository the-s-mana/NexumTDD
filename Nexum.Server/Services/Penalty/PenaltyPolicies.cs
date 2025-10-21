using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPenaltyPolicies
    {
        // Define methods related to penalty policies here
        ProductContact penaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
        Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicies();
        Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id);
    }
    public class PenaltyPolicies : IPenaltyPolicies
    {
        private readonly IPenaltyPoliciesDAC penaltyPoliciesDAC;
        public PenaltyPolicies(IPenaltyPoliciesDAC penaltyPoliciesDAC) 
        { 
            this.penaltyPoliciesDAC = penaltyPoliciesDAC;
        }

        public async Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicies()
        {
            return await penaltyPoliciesDAC.GetAllProductContactAsync();
        }

        public async Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id)
        {
            return await penaltyPoliciesDAC.GetPenaltyPolicyByIdAsync(id);
        }

        public ProductContact penaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
            return penaltyPoliciesDAC.GetPenaltyPolicies(penaltyPoliciesRequest);
        }
    }
    
    
}
