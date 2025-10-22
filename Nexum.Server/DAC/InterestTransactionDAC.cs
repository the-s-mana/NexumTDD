using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.CreditWallet;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.DAC
{
    public interface IInterestTransactionDAC
    {
        void CreateInterestTransactionAsync(CreateInterestTransactionDTO create);
    }

    public class InterestTransactionDAC : IInterestTransactionDAC
    {
        private readonly ISurrealDbProvider<InterestTransaction, CreateInterestTransactionDTO> _interestTransactionDbProvider;
        public InterestTransactionDAC(
            SurrealDbProviderFactoryBase surrealDbProviderFactory
            , ISurrealDbProvider<InterestTransaction, CreateInterestTransactionDTO> interestTransactionDbProvider)
        {
            _interestTransactionDbProvider = surrealDbProviderFactory.Create<InterestTransaction, CreateInterestTransactionDTO>();
        }
        public async void CreateInterestTransactionAsync(CreateInterestTransactionDTO create)
        {
            await _interestTransactionDbProvider.CreateNexum(create);
        }
    }
}
