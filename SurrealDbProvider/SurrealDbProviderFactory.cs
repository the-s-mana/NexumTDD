using ManaApi.Configs;
using Microsoft.Extensions.Options;
using SurrealDb.Net;

namespace ManaApi.Services.SurrealDbProvider
{
    /// <summary>
    /// SurrealDb Provider Factory Base
    /// </summary>
    public abstract class SurrealDbProviderFactoryBase
    {
        /// <summary>
        /// Create db provider for table
        /// </summary>
        /// <typeparam name="TSurrealModel"></typeparam>
        /// <returns></returns>
        public abstract IDbProvider<TSurrealModel, TManaModel> Create<TSurrealModel, TManaModel>();
    }

    /// <summary>
    /// SurrealDb Provider Factory
    /// </summary>
    public sealed class SurrealDbProviderFactory : SurrealDbProviderFactoryBase
    {
        private readonly IServiceProvider provider;
        private readonly ISurrealDbClient surrealDbClient;
        private readonly ApiOptions surrealOption;

        /// <summary>
        /// SurrealDb Provider Factory constructor
        /// </summary>
        public SurrealDbProviderFactory(IServiceProvider provider,
            ISurrealDbClient surrealDbClient,
            IOptionsSnapshot<ApiOptions> apiOpionsAccessor)
        {
            this.provider = provider;
            this.surrealDbClient = surrealDbClient;
            surrealOption = apiOpionsAccessor.Get(ApiOptions.SurrealDb);
            this.surrealDbClient.SignIn(
                new SurrealDb.Net.Models.Auth.DatabaseAuth
                {
                    Username = surrealOption.Username,
                    Password = surrealOption.Password,
                    Namespace = surrealOption.Namespace,
                    Database = surrealOption.Database
                }).Wait();
        }

        /// <inheritdoc />
        public sealed override IDbProvider<TSurrealModel, TManaModel> Create<TSurrealModel, TManaModel>()
        {
            var dbprovider = provider?.GetService<IDbProvider<TSurrealModel, TManaModel>>();
            if (dbprovider == null) throw new ArgumentNullException(nameof(dbprovider));
            return dbprovider;
        }
    }
}
