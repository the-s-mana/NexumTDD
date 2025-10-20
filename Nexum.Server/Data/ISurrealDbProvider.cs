using SurrealDb.Net;

namespace Nexum.Server.Data;
public interface ISurrealDbProvider<TSurrealModel, TNexumModel>
{
    // Surreal
    public abstract Task<IEnumerable<TSurrealModel>> ListSurreal(CancellationToken cancellationToken = default);

    public abstract Task<TSurrealModel> CreateSurreal(TSurrealModel data, CancellationToken cancellationToken = default);

    public abstract Task<TSurrealModel> UpdateSurreal(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);

    // Nexum
    public abstract Task<IEnumerable<TNexumModel>> ListNexum(CancellationToken cancellationToken = default);

    public abstract Task<TNexumModel> CreateNexum(TNexumModel data, CancellationToken cancellationToken = default);

    public abstract Task<TNexumModel> UpdateNexum(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
}