using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace ManaApi.Services.SurrealDbProvider
{
    /// <summary>
    /// The db provider
    /// </summary>
    /// <typeparam name="TsurrealModel">SurrealDbModel</typeparam>
    /// <typeparam name="TmanaModel">ManaDbModel</typeparam>
    public interface IDbProvider<TsurrealModel, TmanaModel>
    //where TsurrealModel : SurrealDbModelBase
    //where TmanaModel : ManaDbModelBase
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string Table { get; }
        /// <summary>
        /// list data in collection
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<TsurrealModel>> List(CancellationToken cancellationToken = default);
        /// <summary>
        /// get one data in collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TsurrealModel?> Get(string id, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel?> GetX(string id, CancellationToken cancellationToken = default);
        /// <summary>
        /// get one data in collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TsurrealModel?> Get(RecordId id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates a record in the database : id can randomize by not assign this field
        /// </summary>
        /// <param name="data">object data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TsurrealModel> Create(TsurrealModel data, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> CreateX(TmanaModel data, CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates or updates a specific record
        /// </summary>
        /// <param name="data">object data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TsurrealModel> Upsert(TsurrealModel data, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> UpsertX(TmanaModel data, CancellationToken cancellationToken = default);
        /// <summary>
        /// Modifies all records in a table, or a specific record
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="data">object data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TsurrealModel> Update(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        public abstract Task<TmanaModel> UpdateX(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        /// <summary>
        /// Modifies all records in a table, or a specific record
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="data">object data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TsurrealModel> Update(RecordId id, Dictionary<string, object?> data, CancellationToken cancellationToken = default);
        /// <summary>
        /// Runs a set of SurrealQL statements against the database
        /// </summary>
        /// <param name="qry">query statements</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>query result : can get value by using GetValue() method</returns>
        public abstract Task<SurrealDbResponse> Query(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Runs a set of SurrealQL statements against the database and return list of TmanaModel
        /// </summary>
        /// <param name="qry">query statements</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>query result : can get value by using GetValue() method</returns>
        public abstract Task<IEnumerable<TmanaModel>> Select(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Runs a set of SurrealQL statements against the database and return first of TmanaModel
        /// </summary>
        /// <param name="qry">query statements</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>query result : can get value by using GetValue() method</returns>
        public abstract Task<TmanaModel> SelectOne(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Runs a set of SurrealQL statements against the database and return list of Ts
        /// </summary>
        /// <param name="qry">query statements</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>query result : can get value by using GetValue() method</returns>
        public abstract Task<IEnumerable<Ts>?> Query<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Runs a set of SurrealQL statements against the database and return first of Ts
        /// </summary>
        /// <typeparam name="Ts"></typeparam>
        /// <param name="qry"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<Ts?> QueryOne<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);
    }
}
