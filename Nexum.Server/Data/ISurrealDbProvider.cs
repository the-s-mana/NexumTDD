using SurrealDb.Net;

namespace Nexum.Server.Data;
public interface ISurrealDbProvider<T>
{
    public abstract Task<IEnumerable<T>> List(CancellationToken cancellationToken = default);
}