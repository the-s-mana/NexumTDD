using SurrealDb.Net.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Nexum.Server.DAC.Providers
{
    public interface IDbProvider<TsurrealModel, TPaneltyModel> where TsurrealModel : Record
    {
        string Table { get; }

        Task<IEnumerable<TsurrealModel>> List(CancellationToken cancellationToken = default);
        Task<TsurrealModel?> Get(string id, CancellationToken cancellationToken = default);
        Task<TPaneltyModel?> GetX(string id, CancellationToken cancellationToken = default);
    }
}