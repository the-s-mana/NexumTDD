using System.ComponentModel;

namespace Nexum.Server.Models.CreditWallet;
public class CreateWalletRequestDTO
{
    [DefaultValue(3000.00)]
    public decimal PrincipalBalance { get; set; }

    [DefaultValue(true)]
    public bool Active { get; set; }

    [DefaultValue("ค้างชำระ")]
    public string? Status { get; set; }
}
