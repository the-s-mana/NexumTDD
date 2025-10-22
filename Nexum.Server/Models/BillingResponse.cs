namespace Nexum.Server.Models
{
    public class BillingResponse
    {
        public string CreditWalletId { get; set; } // รหัสกระเป๋าสินเชื่อ
        public decimal InterestAmount { get; set; } // จำนวนดอกเบี้ยรอบนี้
        public decimal AccumulatedAmount { get; set; } // ยอดดอกเบี้ยสะสม
    }
}
