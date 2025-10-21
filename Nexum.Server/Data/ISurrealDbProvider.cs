namespace Nexum.Server.Data
{
    public interface ISurrealDbProvider<TsurrealModel, TnexumModel>
    {
        Task<IEnumerable<TnexumModel>> ListAsNexumModelAsync(CancellationToken cancellationToken = default);
        Task<TnexumModel> GetByIdAsNexumModelAsync(string id, CancellationToken cancellationToken = default);
        Task<TnexumModel> CreateAsNexumModelAsync(TnexumModel model, CancellationToken cancellationToken = default);
        Task<TnexumModel> UpdateAsNexumModelAsync(string id, Dictionary<string, object?> data, CancellationToken cancellationToken);
        Task<TnexumModel> UpsertAsNexumModelAsync(TnexumModel data,  CancellationToken cancellationToken);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Ts>?> QueryAsync<Ts>(FormattableString qry, IReadOnlyDictionary<string, object?>? parameters = default, CancellationToken cancellationToken = default);


    }
}
