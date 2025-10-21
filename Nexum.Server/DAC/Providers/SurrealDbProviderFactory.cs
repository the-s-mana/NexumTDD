using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using System;
namespace Nexum.Server.DAC.Providers
{
    public sealed class SurrealDbProviderFactory : ISurrealDbProviderFactory
    {
        //private readonly IServiceProvider _provider;
        private readonly IServiceScopeFactory _scopeFactory;
        // Factory จะทำการ SignIn แค่ครั้งเดียวตอนเริ่มต้น
        public SurrealDbProviderFactory(IServiceScopeFactory scopeFactory)
        { _scopeFactory = scopeFactory; }
        public IDbProvider<TSurrealModel, TPaneltyModel> Create<TSurrealModel, TPaneltyModel>() where TSurrealModel : Record
        {
            // 1. สร้าง Scope การทำงานขึ้นมาใหม่สำหรับ Request นี้โดยเฉพาะ
            var scope = _scopeFactory.CreateScope();

            // 2. ดึง Service Provider ของ Scope นั้นออกมา
            var provider = scope.ServiceProvider;

            // 3. ใช้ Provider ของ Scope นั้นเพื่อดึง DbProvider ที่เป็น Scoped ออกมา
            var dbProvider = provider.GetRequiredService<IDbProvider<TSurrealModel, TPaneltyModel>>();

            // หมายเหตุ: เราจะไม่ throw exception เอง แต่จะใช้ GetRequiredService() // ซึ่งจะ throw exception ที่ชัดเจนกว่าถ้าหา Service ไม่เจอ
            return dbProvider;
        }
    }
}