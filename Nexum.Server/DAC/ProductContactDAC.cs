using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.DAC
{
    public interface IProductContactDAC
    {
        ProductContact GetProductContact(int creditWalletId);
    }

    public class ProductContactDAC : IProductContactDAC
    {
        public ProductContact GetProductContact(int creditWalletId)
        {
            ProductContact productContact = new ProductContact
            {
                ProductContactId = 1,
                CreditWalletId = creditWalletId,
                CreditLimit = 1000,
                InterestRate = 15,
                //MinimumPayment = 10,
                InterestType = "PerMonth",
                InterestFreePeriodDays = DateTime.Now.AddDays(30),
                //PenaltyFreePeriodDays = DateTime.Now.AddDays(30),
                Active = true,
                CreateDate = DateTime.Now,
                CreateBy = "System",
                UpdateDate = DateTime.Now,
                UpdateBy = "System",
                PenaltyPolicyx = new PenaltyPolicy
                {
                    PenaltyRate = 10,
                    PenaltyType = "",
                }
            };
            return productContact;
        }
    }
}
