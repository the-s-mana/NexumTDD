using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPenaltyPolicies
    {
        // Define methods related to penalty policies here
        ProductContact penaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
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
    }
    
    
}
