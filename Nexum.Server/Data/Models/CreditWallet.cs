using Mapster;
using Nexum.Data.Models.Common;
using Nexum.Server.Extensions;
using Nexum.Server.Models.CreditWallet;

namespace Nexum.Server.Data.Models;

public class CreditWallet : BaseEntity, IRegister
{
    public decimal PrincipalBalance { get; set; } // ยอดเงินต้นคงเหลือ
    public bool Active { get; set; } // สถานะการใช้งาน
    public string? Status { get; set; } // สถานะกระเป๋า (ค้างชำระ)

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreditWallet, ContactResponseDTO>()
            .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId());

        config.NewConfig<WalletResponseDTO, CreditWallet>()
            .ConstructUsing(src => new CreditWallet())
            .Map(dest => dest.Id, src => src.Id.StringToRecordId<CreditWallet>())
            .Ignore(dest => dest.CreateDate)
            .Ignore(dest => dest.CreateBy)
            .Ignore(dest => dest.UpdateDate)
            .Ignore(dest => dest.UpdateBy);
    }
}