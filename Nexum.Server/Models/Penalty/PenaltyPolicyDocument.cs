using SurrealDb.Net.Models;

namespace Nexum.Server.Models 
{
    public class PenaltyPolicyDocument : Record 
    {
        public string PolicyName { get; set; } = string.Empty; 
        public string PenaltyType { get; set; } = string.Empty; 
        public decimal PenaltyRate { get; set; } 
        public decimal FixedAmount { get; set; } 
        public decimal MaxPenalty { get; set; } 
        public decimal TotalCap { get; set; } 
        public int PenaltyFreePeriodDays { get; set; } 
        public decimal MinimumPaymentRate { get; set; } } }