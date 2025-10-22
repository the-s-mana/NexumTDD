using Microsoft.Extensions.Options;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace Nexum.Server.Infrastructures.Surreal
{
    public abstract class SurrealDbProviderFactoryBase
    {
        public abstract IDbProvider<TSurrealModel, TDomainModel>
            Create<TSurrealModel, TDomainModel>()
            where TSurrealModel : SurrealDb.Net.Models.Record
            where TDomainModel : class;
    }

    public sealed class SurrealDbProviderFactory : SurrealDbProviderFactoryBase
    {
        private readonly IServiceProvider provider;

        public SurrealDbProviderFactory(
            IServiceProvider sp,
            ISurrealDbClient client,
            IOptions<SurrealOptions> opt)
        {
            provider = sp;
            var o = opt.Value;

            client.SignIn(new DatabaseAuth
            {
                Namespace = o.Namespace,
                Database = o.Database,
                Username = o.User,
                Password = o.Password
            }).Wait();
        }

        public override IDbProvider<TSurrealModel, TDomainModel>
            Create<TSurrealModel, TDomainModel>()
        {
            return provider.GetService<IDbProvider<TSurrealModel, TDomainModel>>()
                ?? throw new InvalidOperationException(
                    $"IDbProvider<{typeof(TSurrealModel).Name},{typeof(TDomainModel).Name}> not registered.");
        }
    }
}
