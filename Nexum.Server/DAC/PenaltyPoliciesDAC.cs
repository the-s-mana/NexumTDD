using System.Collections.Generic;
using Nexum.Server.DTO;
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

            if (policy == null)
            {
                throw new KeyNotFoundException($"Penalty policy with ID {penaltyPoliciesRequest.PenaltyPolicyID} not found.");
            }

            //// ใช้ Mapster แมปข้อมูล
            //return policy.Adapt<PenaltyPolicyDto>();

            return new ProductContact
            {
                PenaltyPolicyID = penaltyPoliciesRequest.PenaltyPolicyID,
                PolicyName = policy.PolicyName,
                PenaltyType = policy.PenaltyType,
                PenaltyRate = policy.PenaltyRate,
                PenaltyFixed = policy.PenaltyFixed,
                PenaltyMax = policy.PenaltyMax,
                TotalCap = policy.TotalCap,
                PenaltyFreePeriodDays = policy.PenaltyFreePeriodDays,
                MinimumPaymentRate = policy.MinimumPaymentRate,
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
                    PenaltyFixed = 100m, // 100 บาท = 100
                    TotalCap = 1000.0m,
                    PenaltyFreePeriodDays = 5,
                    MinimumPaymentRate = 10.0m, // 10%
                },
                new ProductContact
                {
                    PenaltyPolicyID = 2,
                    PolicyName = "Fixed Penalty",
                    PenaltyType = "Fixed",
                    PenaltyFixed = 200.0m,
                    MinimumPaymentRate = 10.0m, // 10%
                },
                new ProductContact
                {
                    PenaltyPolicyID = 3,
                    PolicyName = "Percentage Penalty",
                    PenaltyType = "Percentage",
                    PenaltyRate = 2.5m,
                    PenaltyMax = 300.0m,
                    PenaltyFreePeriodDays = 5,
                    MinimumPaymentRate = 10.0m, // 10%
                },
                new ProductContact
                {
                    PenaltyPolicyID = 4,
                    PolicyName = "Special Daily Penalty",
                    PenaltyType = "Daily",
                    PenaltyFixed = 200.0m,
                    PenaltyMax = 400.0m,
                    TotalCap = 1200.0m,
                    PenaltyFreePeriodDays = 2,
                    MinimumPaymentRate = 10.0m, // 10%
                }
            };
        }
    }
}
