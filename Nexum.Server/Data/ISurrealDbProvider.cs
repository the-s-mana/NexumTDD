using SurrealDb.Net;

namespace Nexum.Server.Data;
public interface ISurrealDbProvider
{
    SurrealDbClient Client { get; }
}