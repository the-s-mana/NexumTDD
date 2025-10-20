using SurrealDb.Net.Models;

namespace Nexum.Server.Extensions
{
    public static class RecordIdExtensions
    {
        public static string? GetId(this RecordId id)
        {
            return id?.DeserializeId<string>();
        }
    }
}
