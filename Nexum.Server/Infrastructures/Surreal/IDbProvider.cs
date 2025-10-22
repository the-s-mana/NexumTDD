using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace Nexum.Server.Infrastructures.Surreal
{
    public interface IDbProvider<TSurrealModel, TDomainModel>
        where TSurrealModel : Record
        where TDomainModel : class
    {
        string Table { get; }

        // CRUD (Surreal model)
        Task<IEnumerable<TSurrealModel>> List(CancellationToken cancellationToken = default);
        Task<TSurrealModel?> Get(string id, CancellationToken cancellationToken = default);
        Task<TSurrealModel?> Get(RecordId id, CancellationToken cancellationToken = default);
        Task<TSurrealModel> Create(TSurrealModel data, CancellationToken cancellationToken = default);
        Task<TSurrealModel> Upsert(TSurrealModel data, CancellationToken cancellationToken = default);
        Task<TSurrealModel> Update(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        Task<TSurrealModel> Update(RecordId id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);

        // CRUD (mapped → Domain model)
        Task<TDomainModel?> GetX(string id, CancellationToken cancellationToken = default);
        Task<TDomainModel> CreateX(TDomainModel data, CancellationToken cancellationToken = default);
        Task<TDomainModel> UpsertX(string id, TDomainModel data, CancellationToken cancellationToken = default);
        Task<TDomainModel> UpdateX(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        Task<List<TDomainModel>> ListX(CancellationToken cancellationToken = default);

        // Raw / Select helpers (กลับเป็น Domain model)
        Task<SurrealDbResponse> Query(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        Task<List<TDomainModel>> Select(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        Task<TDomainModel?> SelectOne(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);

        // Raw → Surreal model (กรณีอยากได้ชนิดอื่น)
        Task<IEnumerable<Ts>?> Query<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        Task<Ts?> QueryOne<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
    }
}
