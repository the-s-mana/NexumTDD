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
        Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto);
        Task<PenaltyPolicyDTO> UpdatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto);
        Task<PenaltyPolicyDTO> UpsertPenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto);
        Task<bool> DeletePenaltyPolicyAsync(string id);
    }
    public class PenaltyPolicies : IPenaltyPolicies
    {
        private readonly IPenaltyPoliciesDAC penaltyPoliciesDAC;
        public PenaltyPolicies(IPenaltyPoliciesDAC penaltyPoliciesDAC) 
        { 
            this.penaltyPoliciesDAC = penaltyPoliciesDAC;
        }
        public ProductContact penaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
            return penaltyPoliciesDAC.GetPenaltyPolicies(penaltyPoliciesRequest);
        }
        public async Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicies()
        {
            return await penaltyPoliciesDAC.GetAllProductContactAsync();
        }
        public async Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id)
        {
            return await penaltyPoliciesDAC.GetPenaltyPolicyByIdAsync(id);
        }
        public async Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPoliciesDAC.CreatePenaltyPolicyAsync(penaltyPolicyDto);
        }
        public async Task<PenaltyPolicyDTO> UpdatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPoliciesDAC.UpdatePenaltyPolicyAsync(penaltyPolicyDto);
        }
        public async Task<PenaltyPolicyDTO> UpsertPenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPoliciesDAC.UpsertPenaltyPolicyAsync(penaltyPolicyDto);
        }
        public async Task<bool> DeletePenaltyPolicyAsync(string id)
        {
            return await penaltyPoliciesDAC.DeletePenaltyPolicyAsync(id);
        }
 




    }
    
    
}
