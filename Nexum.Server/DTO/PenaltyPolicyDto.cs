namespace Nexum.Server.DTO
{
    public class PenaltyPolicyDto
    {
        public int PenaltyPolicyID { get; set; }
        public string PolicyName { get; set; }
        public string PenaltyType { get; set; }
        public decimal PenaltyRate { get; set; }
        public decimal PenaltyFixed { get; set; }
        public decimal PenaltyMax { get; set; }
        public decimal TotalCap { get; set; }
        public int PenaltyFreePeriodDays { get; set; }
        public decimal MinimumPaymentRate { get; set; }
    }
}
