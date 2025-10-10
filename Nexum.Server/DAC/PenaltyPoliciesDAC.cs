using System.Collections.Generic;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.DAC
{
    public interface IPenaltyPoliciesDAC
    {
        ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        public ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
            // Search for the policy in the mock list by ID
            var policies = GetMockPenaltyPolicies();
            var policy = policies.Find(p => p.PenaltyPolicyID == penaltyPoliciesRequest.PenaltyPolicyID);

            return policy ?? new ProductContact
            {
                PenaltyPolicyID = penaltyPoliciesRequest.PenaltyPolicyID,
                PolicyName = policy.PolicyName,
                PenaltyType = policy.PenaltyType,
                PenaltyRate = policy.PenaltyRate,
                FixedAmount = policy.FixedAmount,
                MaxPenalty = policy.MaxPenalty,
                TotalCap = policy.TotalCap,
                PenaltyFreePeriodDays = policy.PenaltyFreePeriodDays
            };
        }

        public static List<ProductContact> GetMockPenaltyPolicies()
        {
            return new List<ProductContact>
            {
                new ProductContact
                {
                    PenaltyPolicyID = 1,
                    PolicyName = "Standard Daily Penalty",
                    PenaltyType = "Daily",
                    FixedAmount = 100m, // 100 บาท = 100
                    TotalCap = 1000.0m,
                    PenaltyFreePeriodDays = 5
                },
                new ProductContact
                {
                    PenaltyPolicyID = 2,
                    PolicyName = "Fixed Penalty",
                    PenaltyType = "Fixed",
                    FixedAmount = 200.0m,
                },
                new ProductContact
                {
                    PenaltyPolicyID = 3,
                    PolicyName = "Percentage Penalty",
                    PenaltyType = "Percentage",
                    PenaltyRate = 2.5m,
                    MaxPenalty = 300.0m,
                    PenaltyFreePeriodDays = 5
                },
                new ProductContact
                {
                    PenaltyPolicyID = 4,
                    PolicyName = "Special Daily Penalty",
                    PenaltyType = "Daily",
                    FixedAmount = 200.0m,
                    MaxPenalty = 400.0m,
                    TotalCap = 1200.0m,
                    PenaltyFreePeriodDays = 2
                }
            };
        }
    }
}
