using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace Nexum.Server.Data;

public sealed class SurrealDbProvider<T> : ISurrealDbProvider<T>
        where T : Record
{
    public SurrealDbClient Client { get; }

    private readonly ISurrealDbClient surrealDbClient;
    public SurrealDbProvider(IConfiguration configuration,
            ISurrealDbClient surrealDbClient)
    {
        this.surrealDbClient = surrealDbClient;
    }
    public string Table { get => typeof(T).Name; }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> List(CancellationToken cancellationToken)
    {
        return await surrealDbClient.Select<T>(Table, cancellationToken);
    }
}