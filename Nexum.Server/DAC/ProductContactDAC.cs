using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.CreditWallet;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.DAC
{
    public interface IProductContactDAC
    {
        Task<ContactResponseDTO> CreateContactAsync(ContactResponseDTO create);
        Task<ContactResponseDTO> GetContactByIdAsync(string id);
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

        public async Task<ContactResponseDTO> GetContactByIdAsync(string id)
        {
            return await _contactDbProvider.GetByIdNexum(id);
        }
    }
}
