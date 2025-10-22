using Nexum.Server.Models;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models;

public class PenaltyPolicy : Record
{
    public string Name { get; set; }
}