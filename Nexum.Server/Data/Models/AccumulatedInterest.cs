using Mapster;
using Nexum.Data.Models.Common;
using Nexum.Server.Extensions;
using Nexum.Server.Models.Interest;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models;

public class AccumulatedInterest : BaseEntity, IRegister
{
    public RecordId ProductContactId { get; set; } // อ้างอิงไปที่ ProductContact Id
    public decimal AccumInterestRemain { get; set; } // ดอกเบี้ยสะสมคงเหลือ

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccumulatedInterest, AccumulatedInterestResponseDTO>()
            .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId())
            .Map(dest => dest.ProductContactId, src => src.ProductContactId == null ? null : src.ProductContactId.GetId());

        config.NewConfig<AccumulatedInterestResponseDTO, AccumulatedInterest>()
            .ConstructUsing(src => new AccumulatedInterest())
            .Map(dest => dest.Id, src => src.Id.StringToRecordId<AccumulatedInterest>())
            .Map(dest => dest.ProductContactId, src => src.ProductContactId.StringToRecordId<ProductContact>())
            .Ignore(dest => dest.CreateDate)
            .Ignore(dest => dest.CreateBy)
            .Ignore(dest => dest.UpdateDate)
            .Ignore(dest => dest.UpdateBy);
    }
}