using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Nexum.Server.Infrastructures.Surreal;

public sealed class SurrealClient
{
    private readonly HttpClient _http;
    private readonly string _ns, _db;

    public SurrealClient(IOptions<SurrealOptions> opt, IHttpClientFactory f)
    {
        var o = opt.Value;
        _ns = o.Namespace; _db = o.Database;
        _http = f.CreateClient(nameof(SurrealClient));
        _http.BaseAddress = new Uri(o.Endpoint);
        _http.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{o.User}:{o.Password}")));
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string EnsureUse(string surql)
    {
        var s = surql.TrimStart();
        if (s.StartsWith("USE NS", StringComparison.OrdinalIgnoreCase)) return surql;
        return $"USE NS {_ns} DB {_db}; {surql}";
    }

    public async Task<JsonDocument> QueryAsync(string surql, CancellationToken ct = default)
    {
        var final = EnsureUse(surql);
        using var body = new ByteArrayContent(Encoding.UTF8.GetBytes(final)); // raw (no Content-Type)
        var res = await _http.PostAsync("", body, ct);
        var txt = await res.Content.ReadAsStringAsync(ct);
        res.EnsureSuccessStatusCode();
        return JsonDocument.Parse(txt);
    }

    public async Task<IReadOnlyList<T>> QueryToListAsync<T>(string surql, CancellationToken ct = default)
    {
        using var doc = await QueryAsync(surql, ct);
        var arr = doc.RootElement;
        if (arr.ValueKind != JsonValueKind.Array || arr.GetArrayLength() == 0) return Array.Empty<T>();

        // ถ้ามี statement ไหน error ให้ throw ชัด ๆ
        for (int i = 0; i < arr.GetArrayLength(); i++)
        {
            var it = arr[i];
            if (it.TryGetProperty("status", out var st) && st.GetString() != "OK")
            {
                var msg = it.TryGetProperty("detail", out var d) ? d.ToString()
                        : it.TryGetProperty("information", out var inf) ? inf.ToString()
                        : it.ToString();
                throw new ApplicationException($"SurrealDB error at #{i + 1}: {msg}");
            }
        }

        var last = arr[arr.GetArrayLength() - 1];
        if (!last.TryGetProperty("result", out var result)) return Array.Empty<T>();

        var list = new List<T>();
        if (result.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in result.EnumerateArray())
                list.Add(el.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!);
        }
        else if (result.ValueKind == JsonValueKind.Object)
        {
            list.Add(result.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!);
        }
        return list;
    }
}
