using System.Linq;
using System.Text.Json;
using Mapster;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace Nexum.Server.Infrastructures.Surreal
{
    public class DbProvider<TSurrealModel, TDomainModel> : IDbProvider<TSurrealModel, TDomainModel>
        where TSurrealModel : Record
        where TDomainModel : class
    {
        private readonly ISurrealDbClient surrealDbClient;

        public DbProvider(ISurrealDbClient surrealDbClient)
        {
            this.surrealDbClient = surrealDbClient;
        }

        public string Table => "penalty_policies";

        // ===== CRUD (Surreal) =====

        public async Task<IEnumerable<TSurrealModel>> List(CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.Select<TSurrealModel>(Table, cancellationToken);
        }

        public async Task<TSurrealModel?> Get(string id, CancellationToken cancellationToken = default)
        {
            var rid = new RecordIdOfString(Table, id);
            return await surrealDbClient.Select<TSurrealModel>((Table, int.Parse(id)), cancellationToken);
        }

        public async Task<TSurrealModel?> Get(RecordId id, CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.Select<TSurrealModel>(id, cancellationToken);
        }

        public async Task<TSurrealModel> Create(TSurrealModel data, CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.Create(Table, data, cancellationToken);
        }

        public async Task<TSurrealModel> Upsert(TSurrealModel data, CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.Upsert(data, cancellationToken);
        }

        public async Task<TSurrealModel> Update(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default)
        {
            var thing = RecordId.From(Table, id);
            return await surrealDbClient.Merge<TSurrealModel>(thing, data, cancellationToken);
        }

        public async Task<TSurrealModel> Update(RecordId id, Dictionary<string, object?> data, CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.Merge<TSurrealModel>(id, data, cancellationToken);
        }

        // ===== CRUD (mapped → Domain) =====

        public async Task<TDomainModel?> GetX(string id, CancellationToken cancellationToken = default)
        {
            var rid = RecordId.From(Table, int.Parse(id));
            var x = await surrealDbClient.Select<TSurrealModel>(rid, cancellationToken);
            return x.Adapt<TDomainModel>();
        }

        public async Task<TDomainModel> CreateX(TDomainModel data, CancellationToken cancellationToken = default)
        {
            var srData = data.Adapt<TSurrealModel>();
            var createdSrData = await surrealDbClient.Create(Table, srData, cancellationToken);
            return createdSrData.Adapt<TDomainModel>();
        }

        public async Task<TDomainModel> UpsertX(string id, TDomainModel data, CancellationToken cancellationToken = default)
        {
            var x = data.Adapt<TSurrealModel>();
            var y = await surrealDbClient.Upsert(x, cancellationToken);
            return y.Adapt<TDomainModel>();
        }

        public async Task<TDomainModel> UpdateX(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default)
        {
            var thing = RecordId.From(Table, id);
            var x = await surrealDbClient.Merge<TSurrealModel>(thing, data, cancellationToken);
            return x.Adapt<TDomainModel>();                 
        }

        public async Task<List<TDomainModel>> ListX(CancellationToken cancellationToken = default)
            => (await List(cancellationToken)).Adapt<List<TDomainModel>>();

        // ===== Raw / Select =====

        public async Task<SurrealDbResponse> Query(
            FormattableString qry,
            IReadOnlyDictionary<string, object?>? parameters = default,
            CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
        }

        public async Task<IEnumerable<Ts>?> Query<Ts>(
            FormattableString qry,
            IReadOnlyDictionary<string, object?>? parameters = default,
            CancellationToken cancellationToken = default)
        {
            var docs = await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
            return docs.GetValue<IEnumerable<Ts>?>(0);
        }

        public async Task<Ts?> QueryOne<Ts>(
            FormattableString qry,
            IReadOnlyDictionary<string, object?>? parameters = default,
            CancellationToken cancellationToken = default)
        {
            var docs = await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
            var list = docs.GetValue<IEnumerable<Ts>?>(0);
            return list.FirstOrDefault();
        }

        public async Task<List<TDomainModel>> Select(
            FormattableString qry,
            IReadOnlyDictionary<string, object?>? parameters = default,
            CancellationToken cancellationToken = default)
        {
            var docs = (await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken)).GetValue<IEnumerable<TSurrealModel>>(0);
            var result = docs.Adapt<List<TDomainModel>>();
            return result;
        }

        public async Task<TDomainModel?> SelectOne(
            FormattableString qry,
            IReadOnlyDictionary<string, object?>? parameters = default,
            CancellationToken cancellationToken = default)
        {
            var docs = (await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken)).GetValue<IEnumerable<TSurrealModel>>(0).FirstOrDefault();
            var result = docs.Adapt<TDomainModel>();
            return result;
        }
    }
}
