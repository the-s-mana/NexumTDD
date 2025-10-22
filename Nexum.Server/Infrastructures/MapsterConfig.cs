using Mapster;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty.Surreal;

namespace Nexum.Server.Infrastructures
{
    public class MapsterConfig
    {
        public static void Register()
        {
            TypeAdapterConfig<PenaltyPolicyRecord, ProductContact>
                .NewConfig();
        }
    }
}
