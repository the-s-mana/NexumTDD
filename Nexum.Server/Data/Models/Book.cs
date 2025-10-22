using Mapster;
using Nexum.Server.Extensions;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models;

public class Book : Record, IRegister
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int PublishYear { get; set; }
    public RecordId StoreId { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.ForType<RecordId, RecordId>().MapWith(src => src);

        config.NewConfig<Book, BookResponseDTO>()
            .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId())
            .Map(dest => dest.StoreId, src => src.StoreId == null ? null : src.StoreId.GetId());

        config.NewConfig<BookResponseDTO, Book>()
            .Map(dest => dest.Id, src => src.Id.StringToRecordId<Book>())
            .Map(dest => dest.StoreId, src => src.StoreId.StringToRecordId<Store>());
    }
}