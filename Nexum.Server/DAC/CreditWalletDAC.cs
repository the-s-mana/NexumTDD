using Nexum.Server.Models;

namespace Nexum.Server.DAC
{
    public interface ICreditWalletDAC
    {
        CreditWallet GetCreditWallet(int creditWalletId);
    }

    public class CreditWalletDAC : ICreditWalletDAC
    {
        public CreditWallet GetCreditWallet(int creditWalletId)
        {
            CreditWallet creditWallet = new CreditWallet
            {
                CreditWalletId = creditWalletId,
                PrincipalBalance = 1000,
                Active = true,
                CreateDate = DateTime.Now,
                CreateBy = "System",
                UpdateDate = DateTime.Now,
                UpdateBy = "System"
            };
            return creditWallet;
        }
    }
}
