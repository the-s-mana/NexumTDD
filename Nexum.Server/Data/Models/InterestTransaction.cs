using Mapster;
using Nexum.Data.Models.Common;
using Nexum.Server.Extensions;
using Nexum.Server.Models;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models;

public class InterestTransaction : BaseEntity, IRegister
{
    public RecordId ProductContactId { get; set; } // อ้างอิงไปที่ ProductContact Id
    public decimal InterestAmount { get; set; } // จำนวนดอกเบี้ยรอบนี้
    public decimal AccumulatedAmount { get; set; } // ยอดดอกเบี้ยสะสม
    public string? Remark { get; set; } // หมายเหตุ

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InterestTransaction, CreateInterestTransactionDTO>()
            .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId())
            .Map(dest => dest.ProductContactId, src => src.ProductContactId == null ? null : src.ProductContactId.GetId());

        config.NewConfig<CreateInterestTransactionDTO, InterestTransaction>()
            .ConstructUsing(src => new InterestTransaction())
            .Map(dest => dest.Id, src => src.Id.StringToRecordId<InterestTransaction>())
            .Map(dest => dest.ProductContactId, src => src.ProductContactId.StringToRecordId<ProductContact>())
            .Ignore(dest => dest.CreateDate)
            .Ignore(dest => dest.CreateBy)
            .Ignore(dest => dest.UpdateDate)
            .Ignore(dest => dest.UpdateBy);
    }
}