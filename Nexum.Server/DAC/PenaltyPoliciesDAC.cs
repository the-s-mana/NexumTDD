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
        ProductContact GetPenaltyPoliciesMock(PenaltyPoliciesRequest penaltyPoliciesRequest);
        Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicyAsync();
        Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string Id);
        Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto);
        Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsyncSurreal(PenaltyPolicy penaltyPolicy);
        Task<PenaltyPolicyDTO> UpsertPenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto);
        Task<bool> DeletePenaltyPolicyAsync(string Id);
        Task<PenaltyPolicyDTO> UpdatePenaltyPolicyAsync(string Id, Dictionary<string, object?> dictionary);
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        SurrealDbProviderFactoryBase surrealDbProviderFactory;
        ISurrealDbProvider<PenaltyPolicy, PenaltyPolicyDTO> penaltyPolicyDbProvider;
        public PenaltyPoliciesDAC(SurrealDbProviderFactoryBase surrealDbProviderFactory)
        {
            this.surrealDbProviderFactory = surrealDbProviderFactory;
            penaltyPolicyDbProvider = surrealDbProviderFactory.Create<PenaltyPolicy, PenaltyPolicyDTO>();
        }

        #region Mock Data
        public ProductContact GetPenaltyPoliciesMock(PenaltyPoliciesRequest penaltyPoliciesRequest)
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
                    PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
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
                    PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
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
                    PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
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
                    PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
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
        #endregion

        public async Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicyAsync()
        {
            var policies = await penaltyPolicyDbProvider.ListAsNexumModelAsync();
            return policies.ToList();
        }
        public async Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id)
        {
            var policy = await penaltyPolicyDbProvider.GetByIdAsNexumModelAsync(id);
            return policy;
        }
        public async Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            var createdPolicy = await penaltyPolicyDbProvider.CreateAsNexumModelAsync(penaltyPolicyDto);
            return createdPolicy;
        }
        public async Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsyncSurreal(PenaltyPolicy penaltyPolicy)
        {
            var createdPolicy = await penaltyPolicyDbProvider.CreateAsSurrealModelAsync(penaltyPolicy);
            return createdPolicy;
        }
        public async Task<PenaltyPolicyDTO> UpdatePenaltyPolicyAsync(string Id, Dictionary<string, object?> dictionary)
        {
            var updatedPolicy = await penaltyPolicyDbProvider.UpdateAsNexumModelAsync(Id, dictionary, CancellationToken.None);
            return updatedPolicy;
        }
        public async Task<PenaltyPolicyDTO> UpsertPenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            var upsertedPolicy = await penaltyPolicyDbProvider.UpsertAsNexumModelAsync(penaltyPolicyDto, CancellationToken.None);
            return upsertedPolicy;
        }
        public async Task<bool> DeletePenaltyPolicyAsync(string id)
        {
            return await penaltyPolicyDbProvider.DeleteAsync(id);
        }






    }
}
