namespace Nexum.Server.Models
{
    public class CalculateInterestResponse
    {
        public decimal InterestAmount { get; set; } // จำนวนดอกเบี้ย
        public decimal AccumInterestRemain { get; set; } // ยอดดอกเบี้ยสะสม

    }
}
