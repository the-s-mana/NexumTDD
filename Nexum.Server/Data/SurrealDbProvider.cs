using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace Nexum.Server.Data
{
    public sealed class SurrealDbProvider<T> : ISurrealDbProvider<T>
        where T : Record
    {
        public SurrealDbClient Client { get; }

        private readonly ISurrealDbClient surrealDbClient;
        public SurrealDbProvider(IConfiguration configuration,
                ISurrealDbClient surrealDbClient)
        {
            this.surrealDbClient = surrealDbClient;
        }
        public string Table { get => typeof(T).Name; }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> List(CancellationToken cancellationToken)
        {
            return await surrealDbClient.Select<T>(Table, cancellationToken);
        }

        public async Task<T?> GetById(string id, CancellationToken cancellationToken = default)
        {
            var result = await surrealDbClient.Select<T>(id, cancellationToken);
            return result.FirstOrDefault();
        }

        public async Task<T> Create(T entity, CancellationToken cancellationToken = default)
        {
            var result = await surrealDbClient.Create<T>(Table, entity, cancellationToken);
            return result;
        }

        public async Task<T> Update(string id, T entity, CancellationToken cancellationToken = default)
        {
            var result = await surrealDbClient.Update<T>(id, entity, cancellationToken);
            return result.FirstOrDefault()!;
        }

        public async Task<bool> Delete(string id, CancellationToken cancellationToken = default)
        {
            await surrealDbClient.Delete(id, cancellationToken);
            var entity = await GetById(id, cancellationToken);
            if (entity != null)
                return false;
            return true;
        }
    }
}
