using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models.Common
{
    public abstract class Record
    {
        public RecordId Id { get; set; }
    }
}
