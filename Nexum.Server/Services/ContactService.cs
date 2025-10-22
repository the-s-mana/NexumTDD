using MapsterMapper;
using Nexum.Server.DAC;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;

namespace Nexum.Server.Services
{
    public interface IContactService
    {
        Task<ContactResponseDTO> CreateContactAsync(CreateContactRequestDTO creatRequest);
        Task<ContactResponseDTO> GetContactByWalletIdAsync(string id);
    }
    public class ContactService : IContactService
    {
        private readonly IProductContactDAC _contactDac;
        private readonly IMapper _mapper;

        public ContactService(IProductContactDAC contactDac, IMapper mapper)
        {
            _contactDac = contactDac;
            _mapper = mapper;
        }

        public async Task<ContactResponseDTO> CreateContactAsync(CreateContactRequestDTO creatRequest)
        {
            ContactResponseDTO create = new ContactResponseDTO
            {
                CreditWalletId = creatRequest.CreditWalletId,
                DueDate = creatRequest.DueDate,
                CreditLimit = creatRequest.CreditLimit,
                Active = creatRequest.Active,
                InterestType = creatRequest.InterestType,
                InterestRate = creatRequest.InterestRate,
                MaxInterestRatePerBilling = creatRequest.MaxInterestRatePerBilling,
                InterestFreePeriodDays = creatRequest.InterestFreePeriodDays,
                PenaltyPolicyID = creatRequest.PenaltyPolicyID,
                PolicyName = creatRequest.PolicyName,
                PenaltyType = creatRequest.PenaltyType,
                PenaltyRate = creatRequest.PenaltyRate,
                FixedAmount = creatRequest.FixedAmount,
                MaxPenalty = creatRequest.MaxPenalty,
                TotalCap = creatRequest.TotalCap,
                PenaltyFreePeriodDays = creatRequest.PenaltyFreePeriodDays,
                MinimumPaymentRate = creatRequest.MinimumPaymentRate
            };
            return await _contactDac.CreateContactAsync(create);
        }
        public async Task<ContactResponseDTO> GetContactByWalletIdAsync(string id)
        {
            RecordId walletRecordId = RecordId.From(nameof(CreditWallet), id);
            return await _contactDac.GetContactByWalletIdAsync(walletRecordId);
        }

    }
}
