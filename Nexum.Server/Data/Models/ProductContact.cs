using Mapster;
using Nexum.Data.Models.Common;
using Nexum.Server.Extensions;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;
using System.ComponentModel.DataAnnotations;

namespace Nexum.Server.Data.Models;

public class ProductContact : BaseEntity, IRegister
{
    #region Common
    public RecordId CreditWalletId { get; set; } // อ้างอิงไปที่ CreditWallet Id
    public DateTime DueDate { get; set; } // วันครบกำหนดชำระ
    public decimal CreditLimit { get; set; } // วงเงินสินเชื่อ
    public bool Active { get; set; } // สถานะการใช้งาน
    #endregion

    #region Interest
    public string? InterestType { get; set; } // รูปแบบดอกเบี้ย (PerMonth, PerDay)
    public decimal InterestRate { get; set; } // อัตราดอกเบี้ย
    public decimal MaxInterestRatePerBilling { get; set; } // อัตราดอกเบี้ยสูงสุดต่อรอบบิล
    public DateTime InterestFreePeriodDays { get; set; } // ระยะปลอดดอกเบี้ย วันสิ้นสุด
    #endregion

    #region Penalty
    public RecordId PenaltyPolicyID { get; set; } // รหัสนโยบายค่าปรับ
    public string PolicyName { get; set; } //ชื่อของนโยบาย (เช่น "ค่าปรับรายวันมาตรฐาน")
    public string PenaltyType { get; set; } // ประเภทการคำนวณ ('Daily', 'Fixed', 'Percentage')
    public decimal PenaltyRate { get; set; } // อัตราที่ใช้คำนวณ (อาจเป็นบาท/วัน หรือ %)
    public decimal FixedAmount { get; set; } // ค่าปรับแบบคงที่ (สำหรับประเภท 'Fixed')
    public decimal MaxPenalty { get; set; } // เพดานค่าปรับต่อครั้ง (เช่น 300 บาท)
    public decimal TotalCap { get; set; } // เพดานค่าปรับสะสมสูงสุด (เช่น 1000 บาท)
    public int PenaltyFreePeriodDays { get; set; } // จำนวนวันผ่อนผันหลัง Due Date
    public decimal MinimumPaymentRate { get; set; } // อัตราชำระขั้นต่ำ (%)
    #endregion

    public void Register(TypeAdapterConfig config)
    {
        config.ForType<RecordId, RecordId>().MapWith(src => src);

        config.NewConfig<ProductContact, ContactResponseDTO>()
            .Map(dest => dest.Id, src => src.Id == null ? null : src.Id.GetId())
            .Map(dest => dest.CreditWalletId, src => src.CreditWalletId == null ? null : src.CreditWalletId.GetId())
            .Map(dest => dest.PenaltyPolicyID, src => src.PenaltyPolicyID == null ? null : src.PenaltyPolicyID.GetId());

        config.NewConfig<ContactResponseDTO, ProductContact>()
            .ConstructUsing(src => new ProductContact())
            .Map(dest => dest.Id, src => src.Id.StringToRecordId<ProductContact>())
            .Map(dest => dest.CreditWalletId, src => src.CreditWalletId.StringToRecordId<CreditWallet>())
            .Map(dest => dest.PenaltyPolicyID, src => src.PenaltyPolicyID.StringToRecordId<PenaltyPolicy>())
            .Ignore(dest => dest.CreateDate)
            .Ignore(dest => dest.CreateBy)
            .Ignore(dest => dest.UpdateDate)
            .Ignore(dest => dest.UpdateBy);
    }
}