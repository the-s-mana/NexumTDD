using System.Collections.Generic;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.DAC
{
    public interface IPenaltyPoliciesDAC
    {
        ProductContact1 GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        public ProductContact1 GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
            // Search for the policy in the mock list by ID
            var policies = GetMockPenaltyPolicies();
            var policy = policies.Find(p => p.PenaltyPolicyID == penaltyPoliciesRequest.PenaltyPolicyID);

            if (policy == null)
            {
                throw new KeyNotFoundException($"Penalty policy with ID {penaltyPoliciesRequest.PenaltyPolicyID} not found.");
            }

            return new ProductContact1
            {
                PenaltyPolicyID = penaltyPoliciesRequest.PenaltyPolicyID,
                PolicyName = policy.PolicyName,
                PenaltyType = policy.PenaltyType,
                PenaltyRate = policy.PenaltyRate,
                FixedAmount = policy.FixedAmount,
                MaxPenalty = policy.MaxPenalty,
                TotalCap = policy.TotalCap,
                PenaltyFreePeriodDays = policy.PenaltyFreePeriodDays,
                MinimumPaymentRate = policy.MinimumPaymentRate,
            };
        }

        public static List<ProductContact1> GetMockPenaltyPolicies()
        {
            return new List<ProductContact1>
            {
                new ProductContact1
                {
                    PenaltyPolicyID = 1,
                    PolicyName = "Standard Daily Penalty",
                    PenaltyType = "Daily",
                    FixedAmount = 100m, // 100 บาท = 100
                    TotalCap = 1000.0m,
                    PenaltyFreePeriodDays = 5,
                    MinimumPaymentRate = 10.0m, // 10%
                },
                new ProductContact1
                {
                    PenaltyPolicyID = 2,
                    PolicyName = "Fixed Penalty",
                    PenaltyType = "Fixed",
                    FixedAmount = 200.0m,
                    MinimumPaymentRate = 10.0m, // 10%
                },
                new ProductContact1
                {
                    PenaltyPolicyID = 3,
                    PolicyName = "Percentage Penalty",
                    PenaltyType = "Percentage",
                    PenaltyRate = 2.5m,
                    MaxPenalty = 300.0m,
                    PenaltyFreePeriodDays = 5,
                    MinimumPaymentRate = 10.0m, // 10%
                },
                new ProductContact1
                {
                    PenaltyPolicyID = 4,
                    PolicyName = "Special Daily Penalty",
                    PenaltyType = "Daily",
                    FixedAmount = 200.0m,
                    MaxPenalty = 400.0m,
                    TotalCap = 1200.0m,
                    PenaltyFreePeriodDays = 2,
                    MinimumPaymentRate = 10.0m, // 10%
                }
            };
        }
    }
}
