using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexum.Server.Models
{
    public class CreditWallet : BaseEntity
    {
        [Key]
        public int CreditWalletId { get; set; } // รหัสกระเป๋าสินเชื่อ
        public decimal PrincipalBalance { get; set; } // ยอดเงินต้นคงเหลือ
        public bool Active { get; set; } // สถานะการใช้งาน
        public string? Status { get; set; } // สถานะกระเป๋า (ค้างชำระ)

    }
}
