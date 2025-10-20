using Mapster;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace Nexum.Server.Data;

public sealed class SurrealDbProvider<TSurrealModel, TNexumModel> : ISurrealDbProvider<TSurrealModel, TNexumModel>
        where TSurrealModel : Record
{
    public SurrealDbClient Client { get; }

    private readonly ISurrealDbClient surrealDbClient;
    public SurrealDbProvider(IConfiguration configuration,
            ISurrealDbClient surrealDbClient)
    {
        this.surrealDbClient = surrealDbClient;
    }
    public string Table { get => typeof(TSurrealModel).Name; }

    // Surreal
    public async Task<IEnumerable<TSurrealModel>> ListSurreal(CancellationToken cancellationToken)
    {
        var result = await surrealDbClient.Select<TSurrealModel>(Table, cancellationToken);
        return result;
    }

    public async Task<TSurrealModel> CreateSurreal(TSurrealModel data, CancellationToken cancellationToken)
    {
        var result = await surrealDbClient.Create(Table, data, cancellationToken);
        return result;
    }

    public async Task<TSurrealModel> UpdateSurreal(string id, Dictionary<string, object?> data, CancellationToken cancellationToken)
    {
        var thing = RecordId.From(Table, id);

        return await surrealDbClient.Merge<TSurrealModel>(thing, data, cancellationToken);
    }

    // Nexum
    public async Task<IEnumerable<TNexumModel>> ListNexum(CancellationToken cancellationToken)
    {
        var x = await surrealDbClient.Select<TSurrealModel>(Table, cancellationToken);
        return x.Adapt<IEnumerable<TNexumModel>>();
    }

    public async Task<TNexumModel> CreateNexum(TNexumModel data, CancellationToken cancellationToken)
    {
        var srData = data.Adapt<TSurrealModel>();
        var createdSrData = await surrealDbClient.Create(Table, srData, cancellationToken);
        return createdSrData.Adapt<TNexumModel>();
    }

    public async Task<TNexumModel> UpdateNexum(string id, Dictionary<string, object?> data, CancellationToken cancellationToken)
    {
        var thing = RecordId.From(Table, id);
        var x = await surrealDbClient.Merge<TSurrealModel>(thing, data, cancellationToken);
        return x.Adapt<TNexumModel>();
    }
}