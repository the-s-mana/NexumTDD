using Nexum.Data.Models.Common;

namespace Nexum.Server.Data.Models;

public class CreditWallet : BaseEntity
{
    public decimal PrincipalBalance { get; set; } // ยอดเงินต้นคงเหลือ
    public bool Active { get; set; } // สถานะการใช้งาน
    public string? Status { get; set; } // สถานะกระเป๋า (ค้างชำระ)
}