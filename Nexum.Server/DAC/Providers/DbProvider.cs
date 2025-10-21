using Mapster;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nexum.Server.DAC.Providers
{
    public class DbProvider<TsurrealModel, TPaneltyModel> : IDbProvider<TsurrealModel, TPaneltyModel> where TsurrealModel : Record
    {
        private readonly ISurrealDbClient _surrealDbClient;

        public string Table { get; }

        // เพิ่ม field logger
        private readonly ILogger<DbProvider<TsurrealModel, TPaneltyModel>> _logger;
        private readonly string _httpEndpoint = "http://localhost:8000"; // same as Program.cs
        private readonly string _ns = "test";
        private readonly string _db = "test";
        private readonly string _user = "root"; // or from config
        private readonly string _pass = "root";

        public DbProvider(ISurrealDbClient surrealDbClient,
                  ILogger<DbProvider<TsurrealModel, TPaneltyModel>> logger)
        {
            _surrealDbClient = surrealDbClient;
            _logger = logger;
            Table = ToSnakeCaseTableName(typeof(TsurrealModel));
        }

        public async Task<IEnumerable<TsurrealModel>> List(CancellationToken cancellationToken = default)
        {
            return await _surrealDbClient.Select<TsurrealModel>(Table, cancellationToken);
        }

        public async Task<TsurrealModel?> Get(string id, CancellationToken ct = default)
        {
            //var rd = new RecordIdOfString(Table, id);
            //return await _surrealDbClient.Select<TsurrealModel>((Table, id), ct);

            //สร้าง id ในรูปแบบ table:id (จะตรงกับ DB ถ้า Table ถูกต้อง)
            var sql = $"SELECT * FROM {Table} WHERE id = $id";
            var parameters = new Dictionary<string, object?> { ["id"] = $"{Table}:{id}" };
            _logger.LogInformation("Executing RawQuery. Table={Table} Id={Id}", "penalty_policy", id);
            _logger.LogDebug("SQL: {Sql} Params: {Params}", sql, parameters);

            try
            {
                var response = await _surrealDbClient.RawQuery(sql, parameters, ct);
                try
                {
                    var list = response.GetValue<IEnumerable<TsurrealModel>>(0);
                    if (list != null && list.Any())
                        return default;

                    return list?.First();
                }

                catch (Exception typedEx)
                {
                    _logger.LogDebug(typedEx, "GetValue<IEnumerable<T>> failed - will try raw parsing.");
                }

                // 2) Try get as object and inspect runtime type
                object? rawObj = null;
                try
                {
                    rawObj = response.GetValue<object>(0);
                }
                catch (Exception exGetObj)
                {
                    _logger.LogWarning(exGetObj, "GetValue<object>(0) failed - trying HTTP fallback raw fetch");
                    // Optional: fallback to direct HTTP debug fetch (see FetchRawSqlAsync below)
                    var rawDebug = await FetchRawSqlAsync(sql, ct);
                    _logger.LogWarning("HTTP fallback raw:\n{RawDebug}", rawDebug);
                    return default;
                }

                if (rawObj == null)
                {
                    _logger.LogInformation("Raw response value is null (index 0).");
                    return default;
                }
                // Convert runtime object to JSON text safely
                string jsonText;
                switch (rawObj)
                {
                    case byte[] bytes:
                        jsonText = Encoding.UTF8.GetString(bytes);
                        break;
                    case ReadOnlyMemory<byte> rom:
                        jsonText = Encoding.UTF8.GetString(rom.ToArray());
                        break;
                    case string s:
                        jsonText = s;
                        break;
                    case JsonElement je:
                        jsonText = je.GetRawText();
                        break;
                    default:
                        // Last resort: serialize the object to JSON (may produce surrogate output)
                        try
                        {
                            jsonText = JsonSerializer.Serialize(rawObj);
                        }
                        catch (Exception exSer)
                        {
                            _logger.LogWarning(exSer, "Could not Json-serialize rawObj, returning default.");
                            return default;
                        }
                        break;
                }
                _logger.LogDebug("Raw JSON text (index 0): {JsonText}", jsonText);
                // 3) Parse JSON text and extract actual record:
                // Surreal HTTP /sql returns array of responses: [{ time, status, result: [ <record objects> ] }, ...]
                try
                {
                    using var doc = JsonDocument.Parse(jsonText);
                    var root = doc.RootElement;
                    // If top-level is array, take element 0
                    JsonElement firstEntry;
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                        firstEntry = root[0];
                    else
                        firstEntry = root;
                    // Try to get "result" array
                    if (firstEntry.TryGetProperty("result", out JsonElement resultElem) && resultElem.ValueKind == JsonValueKind.Array && resultElem.GetArrayLength() > 0)
                    {
                        var recordElem = resultElem[0];
                        // Deserialize recordElem into TsurrealModel
                        var modelJson = recordElem.GetRawText();
                        var model = JsonSerializer.Deserialize<TsurrealModel>(modelJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return model;
                    }
                    else
                    {
                        _logger.LogInformation("No 'result' array or it's empty in response JSON.");
                        return default;
                    }
                }
                catch (JsonException jex)
                {
                    _logger.LogWarning(jex, "Failed to parse JSON text.");
                    return default;
                }
            }
            catch (SurrealDb.Net.Exceptions.SurrealDbException sdbEx)
            {
                _logger.LogError(sdbEx, "SurrealDB exception running RawQuery - likely auth or transport issue.");
                // optional: try HTTP fallback for debugging
                try
                {
                    var rawDebug = await FetchRawSqlAsync(sql, ct);
                    _logger.LogError("HTTP fallback raw:\n{RawDebug}", rawDebug);
                }
                catch (Exception httpEx)
                {
                    _logger.LogError(httpEx, "HTTP fallback also failed.");
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Get()");
                throw;
            }
        }

        private async Task<string> FetchRawSqlAsync(string sql, CancellationToken ct = default)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_httpEndpoint) };
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_pass}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            client.DefaultRequestHeaders.Add("Surreal-NS", _ns);
            client.DefaultRequestHeaders.Add("Surreal-DB", _db);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Replace $id parameter if present - safe hack for debugging:
            // (Better: pass real parameters through, but Raw / Post of SQL is fine for debugging)

            var content = new StringContent(sql, Encoding.UTF8, "text/plain");
            var resp = await client.PostAsync("/sql", content, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return $"Status: {(int)resp.StatusCode} {resp.ReasonPhrase}\nBody:\n{body}";
        }

        public async Task<TPaneltyModel?> GetX(string id, CancellationToken cancellationToken = default)
        {
            var surrealModel = await Get(id, cancellationToken);
            if (surrealModel is null)
            {
                return default;
            }
            return surrealModel.Adapt<TPaneltyModel>();
        }

        private static string ToSnakeCaseTableName(Type t)
        {
            var name = t.Name.Replace("Document", ""); // PenaltyPolicy
            // Insert underscore between lower->upper transitions (FooBar -> Foo_Bar)
            var snake = Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2");
            return snake.ToLowerInvariant(); // penalty_policy
        }
    }
}