using Nexum.Server.Models;
using Nexum.Server.DAC;
using Nexum.Server.Services.Penalty;

namespace Nexum.Server.Services

{
    public interface IBillingService
    {
        BillingResponse ProcessAndCalculateBill(BillingRequest billingRequest);

    }
    public class BillingService : IBillingService
    {
        private readonly IInterestService _interestService;
        private readonly ICreditWalletDAC _creditWalletDAC;
        private readonly IProductContactDAC _productContactDAC;

        public BillingService(IInterestService interestService
        , ICreditWalletDAC creditWalletDAC, IProductContactDAC productContactDAC)
        {
            _interestService = interestService;
            _creditWalletDAC = creditWalletDAC;
            _productContactDAC = productContactDAC;
        }

        public BillingResponse ProcessAndCalculateBill(BillingRequest billingRequest)
        {
            //// ดึงข้อมูลกระเป๋าสินเชื่อ และ สัญญาสินเชื่อ
            //CreditWallet creditWallet = _creditWalletDAC.GetCreditWallet(billingRequest.CreditWalletId);
            //ProductContact productContact = _productContactDAC.GetProductContact(billingRequest.CreditWalletId);

            //// เงินต้นคงเหลือมากกว่า 0
            //if (creditWallet.PrincipalBalance > 0)
            //{
            //    // คำนวณค่าปรับ

            //    // คำนวณดอกเบี้ย
            //    CalculateInterestRequest calculateInterestRequest = new CalculateInterestRequest()
            //    {
            //        PrincipalBalance = creditWallet.PrincipalBalance,
            //        InterestRate = productContact.InterestRate,
            //        InterestType = productContact.InterestType,
            //        InterestFreePeriodDays = productContact.InterestFreePeriodDays,
            //        MaxInterestAmount = productContact.MaxInterestRatePerBilling
            //    };
            //    CalculateInterestResponse calculateInterestResponse = _interestService.CalculateInterest(calculateInterestRequest);
            //}

            return new BillingResponse();
        }
    }
}
