using MapsterMapper;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;

namespace Nexum.Server.Services
{
    public interface IWalletService
    {
        Task<WalletResponseDTO> CreateWalletAsync(CreateWalletRequestDTO creatRequest);
        Task<WalletResponseDTO> GetWalletByIdAsync(string id);
    }
    public class WalletService : IWalletService
    {
        private readonly ICreditWalletDAC _walletDac;
        private readonly IMapper _mapper;

        public WalletService(ICreditWalletDAC walletDac, IMapper mapper)
        {
            _walletDac = walletDac;
            _mapper = mapper;
        }

        public async Task<WalletResponseDTO> CreateWalletAsync(CreateWalletRequestDTO creatRequest)
        {
            WalletResponseDTO create = new WalletResponseDTO
            {
                PrincipalBalance = creatRequest.PrincipalBalance,
                Active = creatRequest.Active,
                Status = creatRequest.Status,
            };
            return await _walletDac.CreateWalletAsync(create);
        }
        public async Task<WalletResponseDTO> GetWalletByIdAsync(string id)
        {
            return await _walletDac.GetWalletByIdAsync(id);
        }

    }
}
