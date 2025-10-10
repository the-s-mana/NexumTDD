namespace Nexum.Server.Models
{
    public class CalculateInterestRequest
    {
        public decimal PrincipalBalance { get; set; } // ยอดเงินต้นคงเหลือ
        public decimal InterestRate { get; set; } // อัตราดอกเบี้ย
        public string? InterestType { get; set; } // รูปแบบดอกเบี้ย
        public DateTime InterestFreePeriodDays { get; set; } // ระยะปลอดดอกเบี้ย วันสิ้นสุด
        public int ProductContactId { get; set; } // รหัสสัญญาสินเชื่อ

        public decimal MaxInterestAmount { get; set; } // อัตราดอกเบี้ยสูงสุดต่อรอบบิล

    }
}
