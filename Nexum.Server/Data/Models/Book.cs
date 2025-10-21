using Nexum.Server.Models;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models;

public class Store : SurrealDb.Net.Models.Record
{
    public string Name { get; set; }
}