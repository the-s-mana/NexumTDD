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
        Task<PenaltyPolicy> CreateProductContact(PenaltyPolicy productContact);
        Task<List<PenaltyPolicy>> GetAllProductContactAsync();
        ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        SurrealDbProviderFactoryBase surrealDbProviderFactory;
        ISurrealDbProvider<PenaltyPolicy> penaltyPolicyDbProvider;
        public PenaltyPoliciesDAC(SurrealDbProviderFactoryBase surrealDbProviderFactory)
        {
            this.surrealDbProviderFactory = surrealDbProviderFactory;
            penaltyPolicyDbProvider = surrealDbProviderFactory.Create<PenaltyPolicy>();
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
                PenaltyPolicyx = new PenaltyPolicy
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
                    PenaltyPolicyx = new PenaltyPolicy
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
                    PenaltyPolicyx = new PenaltyPolicy
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
                    PenaltyPolicyx = new PenaltyPolicy
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
                    PenaltyPolicyx = new PenaltyPolicy
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

        public async Task<List<PenaltyPolicy>> GetAllProductContactAsync()
        {
            var policies = await penaltyPolicyDbProvider.List();
            return policies.ToList();
        }

        public async Task<PenaltyPolicy> CreateProductContact(PenaltyPolicy penaltyPolicy)
        {
            if (penaltyPolicy == null)
                throw new ArgumentNullException(nameof(penaltyPolicy));

            var createdProductContact = await penaltyPolicyDbProvider.Create(penaltyPolicy);
            return createdProductContact;
        }
    }
}
