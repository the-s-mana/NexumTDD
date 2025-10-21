using Mapster;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;
using SurrealDb.Net.Models.Response;

namespace Nexum.Server.Data;

public sealed class SurrealDbProvider<TSurrealModel, TNexumModel> : ISurrealDbProvider<TSurrealModel, TNexumModel>
        where TSurrealModel : Record
{
    public SurrealDbClient Client { get; }

    private readonly ISurrealDbClient _surrealDbClient;
    public SurrealDbProvider(IConfiguration configuration, ISurrealDbClient surrealDbClient)
    {
        _surrealDbClient = surrealDbClient;
    }
    public string Table { get => typeof(TSurrealModel).Name; }

    // Surreal
    public async Task<IEnumerable<TSurrealModel>> ListSurreal(CancellationToken cancellationToken = default)
    {
        return await _surrealDbClient.Select<TSurrealModel>(Table, cancellationToken);
    }

    public async Task<TSurrealModel> GetByIdSurreal(string id, CancellationToken cancellationToken = default)
    {
        var thing = RecordId.From(Table, id);

        return await _surrealDbClient.Select<TSurrealModel>(thing, cancellationToken);
    }

    public async Task<TSurrealModel> CreateSurreal(TSurrealModel data, CancellationToken cancellationToken = default)
    {
        return await _surrealDbClient.Create(Table, data, cancellationToken);
    }

    public async Task<TSurrealModel> UpdateSurreal(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default)
    {
        var thing = RecordId.From(Table, id);

        return await _surrealDbClient.Merge<TSurrealModel>(thing, data, cancellationToken);
    }

    public async Task<TSurrealModel> UpsertSurreal(TSurrealModel data, CancellationToken cancellationToken = default)
    {
        return await _surrealDbClient.Upsert(data, cancellationToken);
    }

    public async Task DeleteSurreal(string id, CancellationToken cancellationToken = default)
    {
        var thing = RecordId.From(Table, id);

        await _surrealDbClient.Delete(thing, cancellationToken);
    }

    // Nexum
    public async Task<IEnumerable<TNexumModel>> ListNexum(CancellationToken cancellationToken = default)
    {
        var result = await _surrealDbClient.Select<TSurrealModel>(Table, cancellationToken);
        return result.Adapt<IEnumerable<TNexumModel>>();
    }

    public async Task<TNexumModel> GetByIdNexum(string id, CancellationToken cancellationToken = default)
    {
        var thing = RecordId.From(Table, id);
        var result = await _surrealDbClient.Select<TSurrealModel>(thing, cancellationToken);
        return result.Adapt<TNexumModel>();
    }

    public async Task<TNexumModel> CreateNexum(TNexumModel data, CancellationToken cancellationToken = default)
    {
        var mapData = data.Adapt<TSurrealModel>();
        var result = await _surrealDbClient.Create(Table, mapData, cancellationToken);
        return result.Adapt<TNexumModel>();
    }

    public async Task<TNexumModel> UpdateNexum(string id, Dictionary<string, object?> data, CancellationToken cancellationToken = default)
    {
        var thing = RecordId.From(Table, id);
        var result = await _surrealDbClient.Merge<TSurrealModel>(thing, data, cancellationToken);
        return result.Adapt<TNexumModel>();
    }

    public async Task<TNexumModel> UpsertNexum(TNexumModel data, CancellationToken cancellationToken = default)
    {
        var mapData = data.Adapt<TSurrealModel>();
        var result = await _surrealDbClient.Upsert(mapData, cancellationToken);
        return result.Adapt<TNexumModel>();
    }

    public async Task DeleteNexum(string id, CancellationToken cancellationToken = default)
    {
        var thing = RecordId.From(Table, id);
        await _surrealDbClient.Delete(thing, cancellationToken);
    }

    // Query
    public async Task<SurrealDbResponse> RawQuery(
        string sql,
        IReadOnlyDictionary<string, object?>? parameters = default,
        CancellationToken cancellationToken = default)
    {
        return await _surrealDbClient.RawQuery(sql, parameters, cancellationToken);
    }

    public async Task<IEnumerable<TSurrealModel>> RawQuerySurreal<TSurrealModel>(
        string sql,
        IReadOnlyDictionary<string, object?>? parameters = default,
        CancellationToken cancellationToken = default)
    {
        var documents = await _surrealDbClient.RawQuery(sql, parameters, cancellationToken);

        return documents.GetValue<IEnumerable<TSurrealModel>>(0);
    }

    public async Task<IEnumerable<TNexumModel>> RawQueryNexum<TNexumModel>(
        string sql,
        IReadOnlyDictionary<string, object?>? parameters = default,
        CancellationToken cancellationToken = default)
    {
        var data = (await _surrealDbClient.RawQuery(sql, parameters, cancellationToken)).GetValue<IEnumerable<TSurrealModel>>(0);
        var result = data.Adapt<IEnumerable<TNexumModel>>();
        return result;
    }
}