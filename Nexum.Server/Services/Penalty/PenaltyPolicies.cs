using Mapster;
using Nexum.Server.DAC;
using Nexum.Server.Data.Models;
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
        Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsyncSurreal(PenaltyPolicyDTO penaltyPolicyDto);
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
            return penaltyPoliciesDAC.GetPenaltyPoliciesMock(penaltyPoliciesRequest);
        }
        public async Task<List<PenaltyPolicyDTO>> GetAllPenaltyPolicies()
        {
            return await penaltyPoliciesDAC.GetAllPenaltyPolicyAsync();
        }
        public async Task<PenaltyPolicyDTO> GetPenaltyPolicyByIdAsync(string id)
        {
            return await penaltyPoliciesDAC.GetPenaltyPolicyByIdAsync(id);
        }
        public async Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            return await penaltyPoliciesDAC.CreatePenaltyPolicyAsync(penaltyPolicyDto);
        }
        public async Task<PenaltyPolicyDTO> CreatePenaltyPolicyAsyncSurreal(PenaltyPolicyDTO penaltyPolicyDto)
        {
            PenaltyPolicy entity = penaltyPolicyDto.Adapt<PenaltyPolicy>();
            return await penaltyPoliciesDAC.CreatePenaltyPolicyAsyncSurreal(entity);
        }
        public async Task<PenaltyPolicyDTO> UpdatePenaltyPolicyAsync(PenaltyPolicyDTO penaltyPolicyDto)
        {
            if (string.IsNullOrEmpty(penaltyPolicyDto.Id))
            {
                throw new ArgumentException("PenaltyPolicyDTO must have a valid Id for update.");
            }
            var updateData = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(penaltyPolicyDto.PolicyName))
            {
                updateData.Add("PolicyName", penaltyPolicyDto.PolicyName);
            }
            if (!string.IsNullOrEmpty(penaltyPolicyDto.PenaltyType))
            {
                updateData.Add("PenaltyType", penaltyPolicyDto.PenaltyType);
            }
            if (penaltyPolicyDto.PenaltyRate.HasValue)
            {
                updateData.Add("PenaltyRate", penaltyPolicyDto.PenaltyRate);
            }
            if (penaltyPolicyDto.PenaltyFixed.HasValue)
            {
                updateData.Add("PenaltyFixed", penaltyPolicyDto.PenaltyFixed);
            }
            if (penaltyPolicyDto.PenaltyMax != 0)
            {
                updateData.Add("PenaltyMax", penaltyPolicyDto.PenaltyMax);
            }
            if (penaltyPolicyDto.TotalCap != 0)
            {
                updateData.Add("TotalCap", penaltyPolicyDto.TotalCap);
            }
            if (penaltyPolicyDto.PenaltyFreePeriodDays != 0)
            {
                updateData.Add("PenaltyFreePeriodDays", penaltyPolicyDto.PenaltyFreePeriodDays);
            }
            if (penaltyPolicyDto.MinimumPaymentRate != 0)
            {
                updateData.Add("MinimumPaymentRate", penaltyPolicyDto.MinimumPaymentRate);
            }

            return await penaltyPoliciesDAC.UpdatePenaltyPolicyAsync(penaltyPolicyDto.Id, updateData);
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
