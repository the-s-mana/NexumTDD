using Mapster;
using Nexum.Server.API.Dto;
using Nexum.Server.Infrastructures.Surreal;
using Nexum.Server.Models.Penalty.Surreal;

namespace Nexum.Server.DAC
{
    public interface IIssuersDAC
    {
        Task<List<IssuerResponseDTO>> GetAllAsync();
        Task<IssuerResponseDTO?> GetByIdAsync(int id);
        Task<IssuerResponseDTO> UpsertAsync(IssuerRequestDTO req);
        Task RelateIssuerToPolicyAsync(int issuerId, int policyId);
        Task<List<PenaltyPolicyResponseDTO>> GetPoliciesByIssuerAsync(int issuerId);
    }
    public class IssuersDAC : IIssuersDAC
    {
        private readonly IDbProvider<IssuerRecord, IssuerResponseDTO> _db;
        public IssuersDAC(SurrealDbProviderFactoryBase factory)
        {
            _db = factory.Create<IssuerRecord, IssuerResponseDTO>();
        }
        public Task<List<IssuerResponseDTO>> GetAllAsync()
            => _db.ListX();

        public Task<IssuerResponseDTO?> GetByIdAsync(int id)
            => _db.GetX(id.ToString());

        public async Task<IssuerResponseDTO> UpsertAsync(IssuerRequestDTO req)
        {
            var record = req.Adapt<IssuerRecord>();
            var upserted = await _db.Upsert(record.IssuerID.ToString(), record);
            return upserted.Adapt<IssuerResponseDTO>();
        }

        public async Task RelateIssuerToPolicyAsync(int issuerId, int policyId)
        {
            await _db.Query(
                $@"RELATE issuers:{issuerId} ->offers-> penalty_policies:{policyId};");
        }

        public async Task<List<PenaltyPolicyResponseDTO>> GetPoliciesByIssuerAsync(int issuerId)
        {
            var rows = await _db.Query<PenaltyPolicyRecord>(
                $@"
                SELECT PenaltyPolicyID, PolicyName, PenaltyType, FixedAmount, PenaltyRate,
                MaxPenalty, TotalCap, PenaltyFreePeriodDays, MinimumPaymentRate
                FROM (SELECT id FROM issuers WHERE IssuerID = $issuer LIMIT 1)[0].id
                ->offers->penalty_policies;
                ",
                new Dictionary<string, object?> { ["issuer"] = issuerId }
            );
            return rows?.Adapt<List<PenaltyPolicyResponseDTO>>() ?? new();
        }
    }
}
