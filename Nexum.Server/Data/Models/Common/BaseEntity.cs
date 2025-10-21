using SurrealDb.Net.Models;

namespace Nexum.Data.Models.Common
{
    public abstract class BaseEntity: Record
    {
        public DateTime CreateDate { get; set; }
        public string? CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }

        protected BaseEntity()
        {
            var now = DateTime.Now;
            CreateDate = now;
            UpdateDate = now;
            CreateBy = "System";
            UpdateBy = "System";

        }
    }
}
