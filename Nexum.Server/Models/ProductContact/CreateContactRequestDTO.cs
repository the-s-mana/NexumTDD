using SurrealDb.Net.Models;
using System.ComponentModel;

namespace Nexum.Server.Models.CreditWallet;
public class CreateContactRequestDTO
{
    #region Common

    [DefaultValue("j01mpkhpnolx32ql6r46")]
    public string CreditWalletId { get; set; }

    [DefaultValue("2025-11-01T10:00:00Z")] 
    public DateTime DueDate { get; set; }

    [DefaultValue(5000.00)]
    public decimal CreditLimit { get; set; }

    [DefaultValue(true)]
    public bool Active { get; set; } = true;
    #endregion

    #region Interest
    [DefaultValue("PerMonth")]
    public string InterestType { get; set; }

    [DefaultValue(1.5)]
    public decimal InterestRate { get; set; }

    [DefaultValue(500.00)]
    public decimal MaxInterestRatePerBilling { get; set; }

    [DefaultValue("2025-11-01T10:00:00Z")]
    public DateTime? InterestFreePeriodDays { get; set; }
    #endregion

    #region Penalty
    [DefaultValue("i54aij0crxcer0ybrlzt")]
    public string? PenaltyPolicyID { get; set; }

    [DefaultValue("ทดสอบ")]
    public string? PolicyName { get; set; }

    [DefaultValue("Daily")]
    public string? PenaltyType { get; set; }

    [DefaultValue(50.00)]
    public decimal PenaltyRate { get; set; }

    [DefaultValue(0.00)]
    public decimal FixedAmount { get; set; }

    [DefaultValue(500.00)]
    public decimal MaxPenalty { get; set; }

    [DefaultValue(2000.00)]
    public decimal TotalCap { get; set; }

    [DefaultValue(3)]
    public int PenaltyFreePeriodDays { get; set; }

    [DefaultValue(10.0)]
    public decimal MinimumPaymentRate { get; set; }
    #endregion
}
