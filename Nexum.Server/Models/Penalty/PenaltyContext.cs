namespace Nexum.Server.Models.Penalty
{
    public class PenaltyContext
    {
        public decimal OutstandingBalance { get; set; } // ยอดค้างชำระ
        public int OverdueDays { get; set; } // จำนวนวันที่ผิดนัด
        public decimal Percentage { get; set; } // อัตราค่าปรับ (เช่น 5% = 5)
        public decimal MaxPenalty { get; set; } // ค่าปรับสูงสุดที่สามารถคิดได้
        public decimal FixedAmount { get; set; } // อัตราค่าปรับรายวัน (เช่น 10 บาทต่อวัน) fixedAmount
        public decimal TotalCap { get; internal set; }

        public PenaltyContext() { }
        // สามารถเพิ่มข้อมูลอื่นๆ ที่จำเป็นในอนาคตได้ เช่น ประเภทบัญชี
    }
}
