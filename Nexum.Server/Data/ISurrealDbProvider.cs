using SurrealDb.Net;
using SurrealDb.Net.Models.Response;

namespace Nexum.Server.Data;
public interface ISurrealDbProvider<TSurrealModel, TNexumModel>
{
    // Surreal
    public abstract Task<IEnumerable<TSurrealModel>> ListSurreal(CancellationToken cancellationToken = default);
    public abstract Task<TSurrealModel> GetByIdSurreal(string id, CancellationToken cancellationToken = default);
    public abstract Task<TSurrealModel> CreateSurreal(TSurrealModel data, CancellationToken cancellationToken = default);
    public abstract Task<TSurrealModel> UpdateSurreal(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
    public abstract Task<TSurrealModel> UpsertSurreal(TSurrealModel data, CancellationToken cancellationToken = default);
    public abstract Task DeleteSurreal(string id, CancellationToken cancellationToken = default);

    // Nexum
    public abstract Task<IEnumerable<TNexumModel>> ListNexum(CancellationToken cancellationToken = default);
    public abstract Task<TNexumModel> GetByIdNexum(string id, CancellationToken cancellationToken = default);
    public abstract Task<TNexumModel> CreateNexum(TNexumModel data, CancellationToken cancellationToken = default);
    public abstract Task<TNexumModel> UpdateNexum(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
    public abstract Task<TNexumModel> UpsertNexum(TNexumModel data, CancellationToken cancellationToken = default);
    public abstract Task DeleteNexum(string id, CancellationToken cancellationToken = default);

    // Query
    public abstract Task<SurrealDbResponse> RawQuery(string sql, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<TSurrealModel>> RawQuerySurreal<TSurrealModel>(string sql, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<TNexumModel>> RawQueryNexum<TNexumModel>(string sql, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
}