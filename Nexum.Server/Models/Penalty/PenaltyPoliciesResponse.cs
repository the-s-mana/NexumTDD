namespace Nexum.Server.Models.Penalty
{
    public class PenaltyPoliciesResponse
    {
        public int PenaltyPolicyID { get; set; } // รหัสนโยบายค่าปรับ
        public string PolicyName { get; set; } //ชื่อของนโยบาย (เช่น "ค่าปรับรายวันมาตรฐาน")
        public string PenaltyType { get; set; } // ประเภทการคำนวณ ('Daily', 'Fixed', 'Percentage')
        public decimal Rate { get; set; } // อัตราที่ใช้คำนวณ (อาจเป็นบาท/วัน หรือ %)
        public decimal FixedAmount { get; set; } // ค่าปรับแบบคงที่ (สำหรับประเภท 'Fixed')
        public decimal MaxPenalty { get; set; } // เพดานค่าปรับต่อครั้ง (เช่น 300 บาท)
        public decimal TotalCap { get; set; } // เพดานค่าปรับสะสมสูงสุด (เช่น 1000 บาท)
        public int GracePeriodDays { get; set; } // จำนวนวันผ่อนผันหลัง Due Date
    }
}
