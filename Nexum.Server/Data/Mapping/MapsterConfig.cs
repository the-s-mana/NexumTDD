using Mapster;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Mapping
{
    public class MapsterConfig
    {
        public static void RegisterMappings()
        {
            // Use the static property TypeAdapterConfig.GlobalSettings for static access
            TypeAdapterConfig.GlobalSettings.ForType<RecordId, RecordId>().MapWith(src => src);

            //// เพิ่ม Mapping: PenaltyPolicy → PenaltyPolicyDTO
            TypeAdapterConfig<PenaltyPolicy, PenaltyPolicyDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id.GetId())
                .AfterMapping((src, dest) =>
                {
                    var aaa = src.Id.GetId();
                    var bbb = src.Id.Table;
                });

            ////เพิ่ม Mapping: PenaltyPolicyDTO → PenaltyPolicy
            TypeAdapterConfig<PenaltyPolicyDTO, PenaltyPolicy>.NewConfig()
                .Map(dest => dest.Id, src => RecordId.From(nameof(PenaltyPolicy), src.Id.StringToRecordId()));

            // Use the config variable if it is defined elsewhere, otherwise use GlobalSettings
            //TypeAdapterConfig.GlobalSettings.ForType<RecordId, RecordId>().MapWith(src => src);

            //TypeAdapterConfig.GlobalSettings.NewConfig<Book, BookResponseDTO>()
            //    .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId())
            //    .Map(dest => dest.StoreId, src => src.StoreId == null ? null : src.StoreId.GetId());

            //TypeAdapterConfig.GlobalSettings.NewConfig<BookResponseDTO, Book>()
            //    .Map(dest => dest.Id, src => src.Id.StringToRecordId<Book>())
            //    .Map(dest => dest.StoreId, src => src.StoreId.StringToRecordId<Store>());

            //TypeAdapterConfig.GlobalSettings.NewConfig<CreditWallet, WalletResponseDTO>()
            //    .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId());

            //TypeAdapterConfig.GlobalSettings.NewConfig<WalletResponseDTO, CreditWallet>()
            //    .Map(dest => dest.Id, src => src.Id.StringToRecordId<CreditWallet>());
        }
    }
    public static class RecordIdExtensions
    {
        public static string? GetId(this RecordId id)
        {
            return id?.DeserializeId<string>();
        }
    }

    // Add this extension method to convert string to RecordId for PenaltyPolicy
    public static class StringExtensions
    {
        public static RecordId StringToRecordId(this string id)
        {
            // Use the table name from typeof(PenaltyPolicy) or hardcode if needed
            return RecordId.From(nameof(PenaltyPolicy), id);
        }
    }
}
