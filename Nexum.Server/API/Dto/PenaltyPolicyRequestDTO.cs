namespace Nexum.Server.API.Dto
{
    public sealed class PenaltyPolicyRequestDTO
    {
        public int PenaltyPolicyID { get; set; }
        public string PolicyName { get; set; } = default!;
        public string PenaltyType { get; set; } = default!;
        public decimal? PenaltyRate { get; set; }
        public decimal? FixedAmount { get; set; }
        public decimal? MaxPenalty { get; set; }
        public decimal? TotalCap { get; set; }
        public int? PenaltyFreePeriodDays { get; set; }
        public decimal? MinimumPaymentRate { get; set; }
    }
}
