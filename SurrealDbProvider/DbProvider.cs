using Mapster;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace ManaApi.Services.SurrealDbProvider
{
    /// <inheritdoc />
    public class DbProvider<TsurrealModel, TmanaModel> : IDbProvider<TsurrealModel, TmanaModel>
        where TsurrealModel : /*SurrealDbModelBase*/ Record
        //where TmanaModel : ManaDbModelBase
    {
        private readonly ISurrealDbClient surrealDbClient;
        /// <inheritdoc />

        public DbProvider(ISurrealDbClient surrealDbClient
            )
        {
            this.surrealDbClient = surrealDbClient;
        }

        /// <inheritdoc />
        public string Table { get => typeof(TsurrealModel).Name; }

        /// <inheritdoc />
        public async Task<IEnumerable<TsurrealModel>> List(CancellationToken cancellationToken)
        {
            return await surrealDbClient.Select<TsurrealModel>(Table, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TsurrealModel?> Get(string id, CancellationToken cancellationToken)
        {
            var rd = new RecordIdOfString(Table, id);
            return await surrealDbClient.Select<TsurrealModel>((Table, id), cancellationToken);
        }
        public async Task<TmanaModel?> GetX(string id, CancellationToken cancellationToken)
        {
            var rd = new RecordIdOfString(Table, id);
            var x = await surrealDbClient.Select<TsurrealModel>(rd, cancellationToken);
            return x.Adapt<TmanaModel>();
        }

        /// <inheritdoc />
        public async Task<TsurrealModel?> Get(RecordId id, CancellationToken cancellationToken)
        {
            return await surrealDbClient.Select<TsurrealModel>(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TsurrealModel> Create(TsurrealModel data, CancellationToken cancellationToken)
        {
            return await surrealDbClient.Create(Table, data, cancellationToken);
        }
        /// <inheritdoc />
        public async Task<TmanaModel> CreateX(TmanaModel data, CancellationToken cancellationToken)
        {
            var srData = data.Adapt<TsurrealModel>();
            var createdSrData = await surrealDbClient.Create(Table, srData, cancellationToken);
            return createdSrData.Adapt<TmanaModel>();
        }

        /// <inheritdoc />
        public async Task<TsurrealModel> Upsert(TsurrealModel data, CancellationToken cancellationToken)
        {
            return await surrealDbClient.Upsert(data, cancellationToken);
        }
        public async Task<TmanaModel> UpsertX(TmanaModel data, CancellationToken cancellationToken)
        {
            var x = data.Adapt<TsurrealModel>();
            var y = await surrealDbClient.Upsert(x, cancellationToken);
            return y.Adapt<TmanaModel>();
        }

        /// <inheritdoc />
        public async Task<TsurrealModel> Update(string id, Dictionary<string, object?> data, CancellationToken cancellationToken)
        {
            var thing = RecordId.From(Table, id);

            return await surrealDbClient.Merge<TsurrealModel>(thing, data, cancellationToken);
        }
        public async Task<TmanaModel> UpdateX(string id, Dictionary<string, object?> data, CancellationToken cancellationToken)
        {
            var thing = RecordId.From(Table, id);
            var x = await surrealDbClient.Merge<TsurrealModel>(thing, data, cancellationToken);
            return data.Adapt<TmanaModel>();
        }

        /// <inheritdoc />
        public async Task<TsurrealModel> Update(RecordId id, Dictionary<string, object?> data, CancellationToken cancellationToken)
        {
            return await surrealDbClient.Merge<TsurrealModel>(id, data, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<SurrealDbResponse> Query(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default)
        {
            return await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Ts>?> Query<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default)
        {
            var documents = await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
            return documents.GetValue<IEnumerable<Ts>?>(0);
        }

        /// <inheritdoc />
        public async Task<Ts?> QueryOne<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default)
        {
            var documents = await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken);
            var list = documents.GetValue<IEnumerable<Ts>?>(0);
            return list.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TmanaModel>> Select(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default)
        {
            var data = (await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken)).GetValue<IEnumerable<TsurrealModel>>(0);
            var result = data.Adapt<IEnumerable<TmanaModel>>();
            return result;
        }

        /// <inheritdoc />
        public async Task<TmanaModel> SelectOne(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default)
        {
            var data = (await surrealDbClient.RawQuery(qry.ToString(), parameters, cancellationToken)).GetValue<IEnumerable<TsurrealModel>>(0).FirstOrDefault();
            var result = data.Adapt<TmanaModel>();
            return result;
        }
    }
}
