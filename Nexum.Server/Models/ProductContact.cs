using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Models
{
    public class ProductContact : BaseEntity
    {
        #region Common
        [Key]
        public int ProductContactId { get; set; } // รหัสสัญญาสินเชื่อ (Product Contract Id)
        public int CreditWalletId { get; set; } // อ้างอิงไปที่ CreditWallet Id
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

        // 1. ยังคงเก็บ ID ของ Policy ไว้เป็น Foreign Key
        public int PenaltyPolicyID { get; set; }

        // 2. เพิ่ม Navigation Property เพื่อให้เข้าถึง Object ของ Policy ได้โดยตรง
        [ForeignKey("PenaltyPolicyID")]
        public virtual PenaltyPolicyDTO PenaltyPolicyx { get; set; }

        #endregion
    }
}
