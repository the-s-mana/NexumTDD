namespace Nexum.Server.Data
{
    public interface ISurrealDbProvider<T>
    {
        Task<IEnumerable<T>> List(CancellationToken cancellationToken = default);
        Task<T?> GetById(string id, CancellationToken cancellationToken = default);
        Task<T> Create(T entity, CancellationToken cancellationToken = default);
        Task<T> Update(string id, T entity, CancellationToken cancellationToken = default);
        Task<bool> Delete(string id, CancellationToken cancellationToken = default);
    }
}
