using System.ComponentModel;

namespace Nexum.Server.Models
{
    public class BillingRequest
    {
        [DefaultValue("j01mpkhpnolx32ql6r46")]
        public string CreditWalletId { get; set; } // รหัสกระเป๋าสินเชื่อ
    }
}
