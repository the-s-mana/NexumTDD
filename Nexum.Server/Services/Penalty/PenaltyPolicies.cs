using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPenaltyPolicies
    {
        // Define methods related to penalty policies here
        ProductContact penaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
        Task<List<PenaltyPolicy>> GetAllPenaltyPolicies();
        Task<PenaltyPolicy> CreatePolicies(PenaltyPolicy productContact);
    }
    public class PenaltyPolicies : IPenaltyPolicies
    {
        private readonly IPenaltyPoliciesDAC penaltyPoliciesDAC;
        public PenaltyPolicies(IPenaltyPoliciesDAC penaltyPoliciesDAC) 
        { 
            this.penaltyPoliciesDAC = penaltyPoliciesDAC;
        }

        public async Task<PenaltyPolicy> CreatePolicies(PenaltyPolicy penaltyPolicy)
        {
            return await penaltyPoliciesDAC.CreateProductContact(penaltyPolicy);
        }

        public async Task<List<PenaltyPolicy>> GetAllPenaltyPolicies()
        {
            return await penaltyPoliciesDAC.GetAllProductContactAsync();
        }

        public ProductContact penaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
            return penaltyPoliciesDAC.GetPenaltyPolicies(penaltyPoliciesRequest);
        }
    }
    
    
}
