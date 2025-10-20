using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace ManaApi.Services.SurrealDbProvider
{
    public interface IDbProvider<TsurrealModel, TmanaModel>
    {
        public abstract Task<IEnumerable<TsurrealModel>> List(CancellationToken cancellationToken = default);
        public abstract Task<TsurrealModel?> Get(string id, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel?> GetX(string id, CancellationToken cancellationToken = default);
        public abstract Task<TsurrealModel?> Get(RecordId id, CancellationToken cancellationToken = default);
        public abstract Task<TsurrealModel> Create(TsurrealModel data, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> CreateX(TmanaModel data, CancellationToken cancellationToken = default);
        public abstract Task<TsurrealModel> Upsert(TsurrealModel data, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> UpsertX(TmanaModel data, CancellationToken cancellationToken = default);
        public abstract Task<TsurrealModel> Update(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> UpdateX(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        public abstract Task<TsurrealModel> Update(RecordId id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        public abstract Task<SurrealDbResponse> Query(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<TmanaModel>> Select(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> SelectOne(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<Ts>?> Query<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        public abstract Task<Ts?> QueryOne<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
    }
}
