using SurrealDb.Net.Models;

namespace Nexum.Server.Extensions
{
    public static class MapperExtensions
    {
        public static RecordId? StringToRecordId<T>(this string? recordIdString)
        {
            if (string.IsNullOrEmpty(recordIdString))
            {
                return null;
            }

            var tableName = nameof(T);

            return RecordId.From(tableName, recordIdString);
        }
    }
}
