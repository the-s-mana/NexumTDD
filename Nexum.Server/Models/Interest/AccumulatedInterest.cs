using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexum.Server.Models
{
    public class AccumulatedInterest : BaseEntity
    {
        [Key]
        public int AccumulatedInterestId { get; set; }  // รหัสดอกเบี้ยสะสม

        public int ProductContactId { get; set; } // อ้างอิงไปที่ ProductContact Id
        public decimal AccumInterestRemain { get; set; } // ดอกเบี้ยสะสมคงเหลือ
    }
}
