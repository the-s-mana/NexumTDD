using Nexum.Server.Models;
using SurrealDb.Net.Models;

namespace Nexum.Server.Data.Models;

public class Book : Record
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int PublishYear { get; set; }
    public RecordId StoreId { get; set; }
}