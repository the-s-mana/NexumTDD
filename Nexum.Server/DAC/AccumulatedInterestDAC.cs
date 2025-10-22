using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Models.Interest;
using SurrealDb.Net.Models;
using System.Text;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.DAC
{
    public interface IAccumulatedInterestDAC
    {
        Task<AccumulatedInterestResponseDTO> GetAccumulatedInterestByProductContactIdAsync(RecordId walletId);
        void UpdateAccumulatedInterestAsync(string id, Dictionary<string, object?> data);
    }

    public class AccumulatedInterestDAC : IAccumulatedInterestDAC
    {
        private readonly ISurrealDbProvider<AccumulatedInterest, AccumulatedInterestResponseDTO> _accumulatedDbProvider;

        public AccumulatedInterestDAC(
            SurrealDbProviderFactoryBase surrealDbProviderFactory
            , ISurrealDbProvider<AccumulatedInterest, AccumulatedInterestResponseDTO> accumulatedDbProvider)
        {
            _accumulatedDbProvider = surrealDbProviderFactory.Create<AccumulatedInterest, AccumulatedInterestResponseDTO>();
        }

        public async Task<AccumulatedInterestResponseDTO> GetAccumulatedInterestByProductContactIdAsync(RecordId productContactId)
        {
            var sqlBuilder = new StringBuilder("SELECT * FROM AccumulatedInterest WHERE ProductContactId = $productContactId LIMIT 1");
            var parameters = new Dictionary<string, object?>
            {
                { "productContactId", productContactId }
            };

            var resultsList = await _accumulatedDbProvider.RawQueryNexum<AccumulatedInterestResponseDTO>(
                sqlBuilder.ToString(),
                parameters
            );

            var accumulatedDto = resultsList?.FirstOrDefault();

            return accumulatedDto;
        }

        public async void UpdateAccumulatedInterestAsync(string id, Dictionary<string, object?> data)
        {
            await _accumulatedDbProvider.UpdateNexum(id, data);
        }
    }
}
