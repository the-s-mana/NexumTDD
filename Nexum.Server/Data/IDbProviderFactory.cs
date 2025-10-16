using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SurrealDb.Net;

namespace Nexum.Server.Data
{
    public class IDbProviderFactory
    {
        /// <summary>
        /// SurrealDb Provider Factory Base
        /// </summary>
        public abstract class SurrealDbProviderFactoryBase
        {
            /// <summary>
            /// Create db provider for table
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public abstract ISurrealDbProvider<T> Create<T>();
        }

        /// <summary>
        /// SurrealDb Provider Factory
        /// </summary>
        public sealed class SurrealDbProviderFactory : SurrealDbProviderFactoryBase
        {
            private readonly IServiceProvider provider;
            private readonly ISurrealDbClient surrealDbClient;

            /// <summary>
            /// SurrealDb Provider Factory constructor
            /// </summary>
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

            /// <inheritdoc />
            public sealed override ISurrealDbProvider<T> Create<T>()
            {
                var dbprovider = provider?.GetService<ISurrealDbProvider<T>>();
                if (dbprovider == null) throw new ArgumentNullException(nameof(dbprovider));
                return dbprovider;
            }
        }
    }
}
