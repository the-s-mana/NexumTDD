using Nexum.Server.Models;

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
                PenaltyRate = 10,
                //MinimumPayment = 10,
                InterestType = "PerMonth",
                PenaltyType = "",
                InterestFreePeriodDays = DateTime.Now.AddDays(30),
                //PenaltyFreePeriodDays = DateTime.Now.AddDays(30),
                Active = true,
                CreateDate = DateTime.Now,
                CreateBy = "System",
                UpdateDate = DateTime.Now,
                UpdateBy = "System"
            };
            return productContact;
        }
    }
}
