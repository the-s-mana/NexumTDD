namespace Nexum.Server.Data
{
    public interface ISurrealDbProvider<TsurrealModel, TnexumModel>
    {
        //Task<IEnumerable<TsurrealModel>> ListAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TnexumModel>> ListAsNexumModelAsync(CancellationToken cancellationToken = default);
        

        //Task<TsurrealModel?> GetById(string id, CancellationToken cancellationToken = default);
        Task<TnexumModel> GetByIdAsNexumModelAsync(string id, CancellationToken cancellationToken = default);


    }
}
