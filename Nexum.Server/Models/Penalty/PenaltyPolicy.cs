using SurrealDb.Net.Models;

namespace Nexum.Server.Models.Penalty
{
    public class PenaltyPolicy
    {
        public int PenaltyPolicyID { get; set; } // รหัสนโยบายค่าปรับ
        public string PolicyName { get; set; } // ชื่อของนโยบาย (เช่น "ค่าปรับรายวันมาตรฐาน")
        public string PenaltyType { get; set; } // ประเภทการคำนวณ ('PercentDaily', 'FixedDaily', 'PercentMonthly', 'FixedMonthly')
        public decimal PenaltyRate { get; set; } // อัตราที่ใช้คำนวณ (อาจเป็นบาท/วัน หรือ %)
        public decimal PenaltyFixed { get; set; } // ค่าปรับแบบคงที่ (สำหรับประเภท 'Fixed')
        public decimal PenaltyMax { get; set; } // เพดานค่าปรับต่อครั้ง (เช่น 300 บาท)
        public decimal TotalCap { get; set; } // เพดานค่าปรับสะสมสูงสุด (เช่น 1000 บาท)
        public int PenaltyFreePeriodDays { get; set; } // จำนวนวันผ่อนผันหลัง Due Date
        public decimal MinimumPaymentRate { get; set; } // อัตราชำระขั้นต่ำ (%)
    }
}
