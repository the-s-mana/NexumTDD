namespace Nexum.Server.Models.CreditWallet;
public class WalletResponseDTO : BaseEntityDTO
{
    public string Id { get; set; }
    public decimal PrincipalBalance { get; set; }
    public bool Active { get; set; }
    public string? Status { get; set; }
}
