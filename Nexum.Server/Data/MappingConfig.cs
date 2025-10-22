using Mapster;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Nexum.Server.Data.Models;
using Nexum.Server.Extensions;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            //config.ForType<RecordId, RecordId>().MapWith(src => src);

            //config.NewConfig<Book, BookResponseDTO>()
            //    .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId())
            //    .Map(dest => dest.StoreId, src => src.StoreId == null ? null : src.StoreId.GetId());

            //config.NewConfig<BookResponseDTO, Book>()
            //    .Map(dest => dest.Id, src => src.Id.StringToRecordId<Book>())
            //    .Map(dest => dest.StoreId, src => src.StoreId.StringToRecordId<Store>());

            // ไม่ได้ใช้ ย้ายไปไว้ใน Model แทน
        }
    }
}
