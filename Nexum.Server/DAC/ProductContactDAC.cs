using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;
using System.Text;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.DAC
{
    public interface IProductContactDAC
    {
        Task<ContactResponseDTO> CreateContactAsync(ContactResponseDTO create);
        Task<ContactResponseDTO> GetContactByWalletIdAsync(RecordId walletId);
    }

    public class ProductContactDAC : IProductContactDAC
    {
        private readonly ISurrealDbProvider<ProductContact, ContactResponseDTO> _contactDbProvider;

        public ProductContactDAC(
            SurrealDbProviderFactoryBase surrealDbProviderFactory
            , ISurrealDbProvider<ProductContact, ContactResponseDTO> contactDbProvider)
        {
            _contactDbProvider = surrealDbProviderFactory.Create<ProductContact, ContactResponseDTO>();
        }

        public async Task<ContactResponseDTO> CreateContactAsync(ContactResponseDTO create)
        {
            return await _contactDbProvider.CreateNexum(create);
        }

        public async Task<ContactResponseDTO> GetContactByWalletIdAsync(RecordId walletId)
        {
            var sqlBuilder = new StringBuilder("SELECT * FROM ProductContact WHERE CreditWalletId = $walletId LIMIT 1");
            var parameters = new Dictionary<string, object?>
            {
                { "walletId", walletId }
            };

            var resultsList = await _contactDbProvider.RawQueryNexum<ContactResponseDTO>(
                sqlBuilder.ToString(),
                parameters
            );

            var contactDto = resultsList?.FirstOrDefault();

            return contactDto;
        }
    }
}
