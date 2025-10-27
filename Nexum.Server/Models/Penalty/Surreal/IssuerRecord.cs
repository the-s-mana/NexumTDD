using Mapster;
using Nexum.Server.Infrastructures.Surreal;
using SurrealDb.Net.Models;

namespace Nexum.Server.Models.Penalty.Surreal
{
    [SurrealTable("issuers")]
    public class IssuerRecord : Record
    {
        public int IssuerID { get; set; }
        public string IssuerName { get; set; } = default!;
        public string ContactEmail { get; set; } = default!;
        public string ContactPhone { get; set; } = default!;
    }

    public sealed class IssuerMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<API.Dto.IssuerRequestDTO, IssuerRecord>();
            config.NewConfig<IssuerRecord, API.Dto.IssuerResponseDTO>()
                .Map(d => d.IssuerID, s => s.IssuerID);
        }
    }
}
