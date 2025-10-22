using MapsterMapper;
using Nexum.Server.DAC;
using Nexum.Server.Models.CreditWallet;

namespace Nexum.Server.Services
{
    public interface IContactService
    {
        Task<ContactResponseDTO> CreateContactAsync(CreateContactRequestDTO creatRequest);
        Task<ContactResponseDTO> GetContactByIdAsync(string id);
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
        public async Task<ContactResponseDTO> GetContactByIdAsync(string id)
        {
            return await _contactDac.GetContactByIdAsync(id);
        }

    }
}
