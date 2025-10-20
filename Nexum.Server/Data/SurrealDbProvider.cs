using Mapster;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace Nexum.Server.Data
{
    public sealed class SurrealDbProvider<TsurrealModel, TnexumModel> : ISurrealDbProvider<TsurrealModel, TnexumModel>
        where TsurrealModel : Record
    {
        public SurrealDbClient Client { get; }

        private readonly ISurrealDbClient surrealDbClient;
        public SurrealDbProvider(IConfiguration configuration, ISurrealDbClient surrealDbClient)
        {
            this.surrealDbClient = surrealDbClient;
        }
        public string Table { get => typeof(TsurrealModel).Name; }


        public async Task<IEnumerable<TsurrealModel>> ListAsync(CancellationToken cancellationToken)
        {
            var results = await surrealDbClient.Select<TsurrealModel>(Table, cancellationToken);
            // ตรวจสอบค่าของ Id ใน results
            foreach (var item in results)
            {
                Console.WriteLine(item.Id); // หรือ debug ดูค่า
            }
            return results;
        }
        public async Task<IEnumerable<TnexumModel>> ListAsNexumModelAsync(CancellationToken cancellationToken = default)
        {
            var entities = await ListAsync(cancellationToken);
            return entities.Adapt<IEnumerable<TnexumModel>>(); // ใช้ Mapster แมป
        }
        public async Task<TsurrealModel?> GetById(string id, CancellationToken cancellationToken = default)
        {
            // Use RecordId.From to convert string id to RecordId
            RecordId recordId = RecordId.From(Table, id);

            var result = await surrealDbClient.Select<TsurrealModel>(recordId, cancellationToken);

            if (result == null)
            {
                return null;
            }

            return result;
        }
        public async Task<TnexumModel> GetByIdAsNexumModelAsync(string id, CancellationToken cancellationToken = default)
        {
            var entities = await GetById(id);
            return entities.Adapt<TnexumModel>(); // ใช้ Mapster แมป
        }





        //public async Task<TsurrealModel> Create(TsurrealModel entity, CancellationToken cancellationToken = default)
        //{
        //    var result = await surrealDbClient.Create<TsurrealModel>(Table, entity, cancellationToken);
        //    return result;
        //}






    }
}
