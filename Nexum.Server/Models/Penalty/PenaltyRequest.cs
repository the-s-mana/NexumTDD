using System.ComponentModel.DataAnnotations;

namespace Nexum.Server.Models.Penalty
{
    public enum AccountStatus
    {
        Active,
        Inactive
    }

    public class PenaltyRequest
    {
        [Required]
        public int UserId { get; set; } //รหัสบัญชี (คีย์หลัก)

        [Required]
        public string PenaltyPolicyID { get; set; }

        [Required]
        [RegularExpression("Active|Inactive", ErrorMessage = "ActiveStatus ต้องเป็น Active หรือ Inactive เท่านั้น")]
        public string ActiveStatus { get; set; }

        [Required]
        public decimal OutstandingBalance { get; set; } //ยอดค้างชำระปัจจุบัน

        [Required]
        public DateTime DueDate { get; set; } //วันครบกำหนดชำระรอบล่าสุด

        [Required]
        public decimal PaymentAmount { get; set; } //ยอดชำระที่ลูกค้าชำระเข้ามา
    }
}
