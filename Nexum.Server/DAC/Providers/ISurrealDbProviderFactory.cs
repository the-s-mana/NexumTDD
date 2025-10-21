using SurrealDb.Net.Models;

namespace Nexum.Server.DAC.Providers
{
    public interface ISurrealDbProviderFactory
    {
        IDbProvider<TSurrealModel, TPaneltyModel> Create<TSurrealModel, TPaneltyModel>() where TSurrealModel : Record;
    }
}