using System.Collections.Generic;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.DAC
{
    public interface IPenaltyPoliciesDAC
    {
        ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
        Task<List<PenaltyPolicyDTO>> GetAllProductContactAsync();
        Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id);
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        SurrealDbProviderFactoryBase surrealDbProviderFactory;
        ISurrealDbProvider<PenaltyPolicy,PenaltyPolicyDTO> penaltyPolicyDbProvider;
        public PenaltyPoliciesDAC(SurrealDbProviderFactoryBase surrealDbProviderFactory)
        {
            this.surrealDbProviderFactory = surrealDbProviderFactory;
            penaltyPolicyDbProvider = surrealDbProviderFactory.Create<PenaltyPolicy,PenaltyPolicyDTO>();
        }

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

            return new ProductContact()
            {
                PenaltyPolicyID = policy.PenaltyPolicyID,
                PenaltyPolicyx = new PenaltyPolicyDTO
                {
                    PolicyName = policy.PenaltyPolicyx.PolicyName,
                    PenaltyType = policy.PenaltyPolicyx.PenaltyType,
                    PenaltyRate = policy.PenaltyPolicyx.PenaltyRate,
                    PenaltyFixed = policy.PenaltyPolicyx.PenaltyFixed,
                    PenaltyMax = policy.PenaltyPolicyx.PenaltyMax,
                    TotalCap = policy.PenaltyPolicyx.TotalCap,
                    PenaltyFreePeriodDays = policy.PenaltyPolicyx.PenaltyFreePeriodDays,
                    MinimumPaymentRate = policy.PenaltyPolicyx.MinimumPaymentRate,
                }
            };
            
        }

        public static List<ProductContact> GetMockPenaltyPolicies()
        {
            return new List<ProductContact>
            {
                new ProductContact
                {
                    PenaltyPolicyID = 1,
                    PenaltyPolicyx = new PenaltyPolicyDTO
                    {
                        PolicyName = "Standard Daily Penalty",
                        PenaltyType = "Daily",
                        PenaltyFixed = 100m, // 100 บาท = 100
                        TotalCap = 1000.0m,
                        PenaltyFreePeriodDays = 5,
                        MinimumPaymentRate = 10.0m, // 10%
                    }
                },
                new ProductContact
                {
                    PenaltyPolicyID = 2,
                    PenaltyPolicyx = new PenaltyPolicyDTO
                    {
                        PolicyName = "Fixed Penalty",
                        PenaltyType = "Fixed",
                        PenaltyFixed = 200.0m,
                        MinimumPaymentRate = 10.0m, // 10%
                    }
                },
                new ProductContact
                {
                    PenaltyPolicyID = 3,
                    PenaltyPolicyx = new PenaltyPolicyDTO
                    {
                        PolicyName = "Percentage Penalty",
                        PenaltyType = "Percentage",
                        PenaltyRate = 2.5m,
                        PenaltyMax = 300.0m,
                        PenaltyFreePeriodDays = 5,
                        MinimumPaymentRate = 10.0m, // 10%
                    }
                },
                new ProductContact
                {
                    PenaltyPolicyID = 4,
                    PenaltyPolicyx = new PenaltyPolicyDTO
                    {
                        PolicyName = "Special Daily Penalty",
                        PenaltyType = "Daily",
                        PenaltyFixed = 200.0m,
                        PenaltyMax = 400.0m,
                        TotalCap = 1200.0m,
                        PenaltyFreePeriodDays = 2,
                        MinimumPaymentRate = 10.0m, // 10%
                    }
                }
            };
        }

        public async Task<List<PenaltyPolicyDTO>> GetAllProductContactAsync()
        {
            var policies = await penaltyPolicyDbProvider.ListAsNexumModelAsync();
            return policies.ToList();
        }
        public async Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id)
        {
            var policy = await penaltyPolicyDbProvider.GetByIdAsNexumModelAsync(id);
            return policy;
        }

    }
}
