using Mapster;
using Nexum.Server.Data.Models;
using Nexum.Server.Models.Penalty;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Mapping
{
    public class MapsterConfig
    {
        public static void RegisterMappings()
        {
            // เพิ่ม Mapping: PenaltyPolicy → PenaltyPolicyDTO
            TypeAdapterConfig<PenaltyPolicy, PenaltyPolicyDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id.GetId())
                .AfterMapping((src, dest) =>
                {
                    var aaa = src.Id.GetId();
                    var bbb = src.Id.Table;
                });

            // เพิ่ม Mapping: PenaltyPolicyDTO → PenaltyPolicy
            TypeAdapterConfig<PenaltyPolicyDTO, PenaltyPolicy>.NewConfig()
                .Map(dest => dest.Id, src => RecordId.From(nameof(PenaltyPolicy), src.Id));



        }
    }
    public static class RecordIdExtensions
    {
        public static string? GetId(this RecordId id)
        {
            return id?.DeserializeId<string>();
        }
    }
}
