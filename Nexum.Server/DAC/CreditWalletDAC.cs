using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.DAC
{
    public interface ICreditWalletDAC
    {
        Task<WalletResponseDTO> CreateWalletAsync(WalletResponseDTO create);
        Task<WalletResponseDTO> GetWalletByIdAsync(string id);
    }

    public class CreditWalletDAC : ICreditWalletDAC
    {
        private readonly ISurrealDbProvider<CreditWallet, WalletResponseDTO> _creditWalletDbProvider;

        public CreditWalletDAC(
            SurrealDbProviderFactoryBase surrealDbProviderFactory
            , ISurrealDbProvider<CreditWallet, WalletResponseDTO> creditWalletDbProvider)
        {
            _creditWalletDbProvider = surrealDbProviderFactory.Create<CreditWallet, WalletResponseDTO>();
        }

        public async Task<WalletResponseDTO> CreateWalletAsync(WalletResponseDTO create)
        {
            return await _creditWalletDbProvider.CreateNexum(create);
        }

        public async Task<WalletResponseDTO> GetWalletByIdAsync(string id)
        {
            return await _creditWalletDbProvider.GetByIdNexum(id);
        }
    }
}
