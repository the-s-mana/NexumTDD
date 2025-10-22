using SurrealDb.Net.Models;
using System.ComponentModel;

namespace Nexum.Server.Models.CreditWallet;
public class ContactResponseDTO : BaseEntityDTO
{
    #region Common

    public string Id { get; set; }
    public string CreditWalletId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal CreditLimit { get; set; }
    public bool Active { get; set; } = true;
    #endregion

    #region Interest
    public string InterestType { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MaxInterestRatePerBilling { get; set; }
    public DateTime? InterestFreePeriodDays { get; set; }
    #endregion

    #region Penalty
    public string? PenaltyPolicyID { get; set; }
    public string? PolicyName { get; set; }
    public string? PenaltyType { get; set; }
    public decimal PenaltyRate { get; set; }
    public decimal FixedAmount { get; set; }
    public decimal MaxPenalty { get; set; }
    public decimal TotalCap { get; set; }
    public int PenaltyFreePeriodDays { get; set; }
    public decimal MinimumPaymentRate { get; set; }
    #endregion
}
