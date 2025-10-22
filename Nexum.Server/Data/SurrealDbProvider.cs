using Mapster;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nexum.Server.Data
{
    public sealed class SurrealDbProvider<TsurrealModel, TnexumModel> : ISurrealDbProvider<TsurrealModel, TnexumModel>
        where TsurrealModel : Record
    {
        public SurrealDbClient Client { get; }

        private readonly ISurrealDbClient surrealDbClient;
        public SurrealDbProvider(IConfiguration configuration, ISurrealDbClient surrealDbClient)
        {
            this.surrealDbClient = surrealDbClient;
        }
        public string Table { get => typeof(TsurrealModel).Name; }

        public async Task<IEnumerable<TnexumModel>> ListAsNexumModelAsync(CancellationToken cancellationToken = default)
        {
            var entities = await surrealDbClient.Select<TsurrealModel>(Table, cancellationToken);
            return entities.Adapt<IEnumerable<TnexumModel>>(); // ใช้ Mapster แมป
        }
        public async Task<TnexumModel> GetByIdAsNexumModelAsync(string id, CancellationToken cancellationToken = default)
        {
            RecordId recordId = RecordId.From(Table, id);
            var entities = await surrealDbClient.Select<TsurrealModel>(recordId, cancellationToken);
            return entities.Adapt<TnexumModel>(); // ใช้ Mapster แมป
        }
        public async Task<TnexumModel> CreateAsNexumModelAsync(TnexumModel entity, CancellationToken cancellationToken = default)
        {
            var surrealEntity = entity.Adapt<TsurrealModel>();
            var createdEntity = await surrealDbClient.Create<TsurrealModel>(Table, surrealEntity, cancellationToken);
            return createdEntity.Adapt<TnexumModel>();
        }
        public async Task<TnexumModel> CreateAsSurrealModelAsync(TsurrealModel model, CancellationToken cancellationToken = default)
        {
            var createdEntity = await surrealDbClient.Create<TsurrealModel>(Table, model, cancellationToken);
            return createdEntity.Adapt<TnexumModel>();
        }
        public async Task<TnexumModel> UpdateAsNexumModelAsync(string id, Dictionary<string, object?> data, CancellationToken cancellationToken)
        {
            var recordId = RecordId.From(Table, id);
            var x = await surrealDbClient.Merge<TsurrealModel>(recordId, data, cancellationToken);
            return x.Adapt<TnexumModel>();
        }
        public async Task<TnexumModel> UpsertAsNexumModelAsync(TnexumModel data, CancellationToken cancellationToken)
        {
            var surrealEntity = data.Adapt<TsurrealModel>();
            var upsertedEntity = await surrealDbClient.Upsert<TsurrealModel>(Table, surrealEntity, cancellationToken);
            return upsertedEntity.Adapt<TnexumModel>();
        }
        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            RecordId recordId = RecordId.From(Table, id);
            return await surrealDbClient.Delete(recordId, cancellationToken);
        }
        public async Task<IEnumerable<Ts>?> QueryAsync<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default)
        {
            var documents = await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
            return documents.GetValue<IEnumerable<Ts>?>(0);
        }

        
    }
}
