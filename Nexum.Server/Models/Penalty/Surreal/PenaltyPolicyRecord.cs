using System.Text.Json.Serialization;
using Mapster;
using SurrealDb.Net.Models;
namespace Nexum.Server.Models.Penalty.Surreal
{
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

        //TODO : Mapster Config
        //public static void MapsterConfig()
        //{
        //    TypeAdapterConfig<PenaltyPolicyRecord, ManaDb.UserInfo>.NewConfig()
        //        .Map(m => m.Id, s => s.Id == null ? null : s.Id.GetId());

        //    TypeAdapterConfig<ManaDb.UserInfo, UserInfo>.NewConfig()
        //        .Ignore(s => s.Id)
        //        .AfterMapping((m, s) => s.Id = string.IsNullOrWhiteSpace(m.Id) ? null : RecordId.From(nameof(UserInfo), m.Id));
        //}
    }
}
