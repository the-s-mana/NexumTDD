using Nexum.Server.DAC;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Services.Penalty;
using SurrealDb.Net.Models;

namespace Nexum.Server.Services

{
    public interface IBillingService
    {
        Task<BillingResponse> ProcessAndCalculateBill(BillingRequest billingRequest);
    }
    public class BillingService : IBillingService
    {
        private readonly IInterestService _interestService;
        private readonly IWalletService _walletService;
        private readonly IContactService _contactService;

        public BillingService(IInterestService interestService,
            IWalletService walletService,
            IContactService contactService)
        {
            _interestService = interestService;
            _walletService = walletService;
            _contactService = contactService;
        }

        public async Task<BillingResponse> ProcessAndCalculateBill(BillingRequest billingRequest)
        {
            // ดึงข้อมูลกระเป๋าสินเชื่อ และ สัญญาสินเชื่อ
            WalletResponseDTO walletDTO = await _walletService.GetWalletByIdAsync(billingRequest.CreditWalletId);
            ContactResponseDTO contactDTO = await _contactService.GetContactByWalletIdAsync(billingRequest.CreditWalletId);

            if (walletDTO == null)
            {
                throw new Exception($"ไม่พบกระเป๋าสินเชื่อที่มี ID: {billingRequest.CreditWalletId}");
            }

            if (contactDTO == null)
            {
                throw new Exception($"ไม่พบสัญญาสินเชื่อที่มี ID: {billingRequest.CreditWalletId}");
            }

            BillingResponse billingResponse = new BillingResponse();
            // เงินต้นคงเหลือมากกว่า 0
            if (walletDTO.PrincipalBalance > 0)
            {
                // คำนวณค่าปรับ

                // คำนวณดอกเบี้ย
                CalculateInterestRequest calculateInterestRequest = new CalculateInterestRequest()
                {
                    PrincipalBalance = walletDTO.PrincipalBalance,
                    InterestRate = contactDTO.InterestRate,
                    InterestType = contactDTO.InterestType,
                    InterestFreePeriodDays = contactDTO.InterestFreePeriodDays ?? default(DateTime),
                    MaxInterestAmount = contactDTO.MaxInterestRatePerBilling,
                    ProductContactId = contactDTO.Id
                };
                CalculateInterestResponse calculateInterestResponse = await _interestService.CalculateInterest(calculateInterestRequest);
                billingResponse.CreditWalletId = billingRequest.CreditWalletId;
                billingResponse.InterestAmount = calculateInterestResponse.InterestAmount;
            }

            billingResponse.CreditWalletId = billingRequest.CreditWalletId;

            return billingResponse;
        }
    }
}
