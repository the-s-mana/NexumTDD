using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SurrealDb.Net;

namespace Nexum.Server.Data
{
    public class IDbProviderFactory
    {
        public abstract class SurrealDbProviderFactoryBase
        {
            public abstract ISurrealDbProvider<TSurrealModel, TNexumModel> Create<TSurrealModel, TNexumModel>();
        }

        public sealed class SurrealDbProviderFactory : SurrealDbProviderFactoryBase
        {
            private readonly IServiceProvider provider;
            private readonly ISurrealDbClient surrealDbClient;

            public SurrealDbProviderFactory(IServiceProvider provider,
                ISurrealDbClient surrealDbClient,
                IConfiguration configuration)
            {
                this.provider = provider;
                this.surrealDbClient = surrealDbClient;


                var settings = configuration.GetSection("SurrealDbSettings");
                var username = settings["Username"]!;
                var password = settings["Password"]!;
                var ns = settings["Namespace"]!;
                var db = settings["Database"]!;
                this.surrealDbClient.SignIn(
                    new SurrealDb.Net.Models.Auth.DatabaseAuth
                    {
                        Username = username,
                        Password = password,
                        Namespace = ns,
                        Database = db
                    }).Wait();
            }

            public sealed override ISurrealDbProvider<TSurrealModel, TNexumModel> Create<TSurrealModel, TNexumModel>()
            {
                var dbprovider = provider?.GetService<ISurrealDbProvider<TSurrealModel, TNexumModel>>();
                if (dbprovider == null) throw new ArgumentNullException(nameof(dbprovider));
                return dbprovider;
            }
        }
    }
}
