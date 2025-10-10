using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexum.Server.Models
{
    public class InterestTransaction : BaseEntity
    {
        [Key]
        public int InterestTransactionId { get; set; } // รหัสรายการดอกเบี้ย (Statement Interest Id)

        public int ProductContactId { get; set; } // อ้างอิงไปที่ ProductContact Id
        public decimal InterestAmount { get; set; } // จำนวนดอกเบี้ยรอบนี้
        public decimal AccumulatedAmount { get; set; } // ยอดดอกเบี้ยสะสม

        public string? Remark { get; set; } // หมายเหตุ
    }
}
