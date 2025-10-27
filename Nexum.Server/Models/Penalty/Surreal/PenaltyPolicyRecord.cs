using System.Text.Json.Serialization;
using Mapster;
using Nexum.Server.API.Dto;
using Nexum.Server.Infrastructures.Surreal;
using SurrealDb.Net.Models;
namespace Nexum.Server.Models.Penalty.Surreal
{
    [SurrealTable("penalty_policies")]
    public class PenaltyPolicyRecord : Record
    {
        public int PenaltyPolicyID { get; set; }
        public string PolicyName { get; set; } = default!;
        public string PenaltyType { get; set; } = default!;
        public double PenaltyRate { get; set; }
        public double FixedAmount { get; set; }
        public double MaxPenalty { get; set; }
        public double TotalCap { get; set; }
        public int PenaltyFreePeriodDays { get; set; }
        public double MinimumPaymentRate { get; set; }
    }
    
    public sealed class PenaltyPolicyMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PenaltyPolicyRequestDTO, PenaltyPolicyRecord>();

            config.NewConfig<PenaltyPolicyRecord, PenaltyPolicyResponseDTO>()
                .Map(d => d.PenaltyPolicyID, s => s.PenaltyPolicyID);
        }
    }
}
