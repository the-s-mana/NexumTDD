using Mapster;
using MapsterMapper;
using Nexum.Server.DAC;
using Nexum.Server.DAC.Providers;
using Nexum.Server.Models;
using Nexum.Server.Services;
using Nexum.Server.Services.Penalty;
using SurrealDb.Net;
using SurrealDb.Net.Models;
//using Dahomey.Cbor;
//using Dahomey.Cbor.Serialization;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DAC services
builder.Services.AddScoped<ICreditWalletDAC, CreditWalletDAC>();
builder.Services.AddScoped<IProductContactDAC, ProductContactDAC>();
builder.Services.AddScoped<IAccumulatedInterestDAC, AccumulatedInterestDAC>();
builder.Services.AddScoped<IInterestTransactionDAC, InterestTransactionDAC>();
builder.Services.AddScoped<IPenaltyPoliciesDAC, PenaltyPoliciesDAC>();

// Register Service services
builder.Services.AddScoped<IInterestService, InterestService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IPenalty, Penalty>();
builder.Services.AddScoped<IDailyPenalty, DailyPenalty>();
builder.Services.AddScoped<IPercentagePenalty, PercentagePenalty>();
builder.Services.AddScoped<IPenaltyPolicies, PenaltyPolicies>();
builder.Services.AddScoped<IFixedPenalty, FixedPenalty>();

//########################### ส่วนที่เต้เพิ่มเข้ามา ########################### 

// Register Utils
//builder.Services.AddScoped<IDateTimeUtils, DateTimeUtils>();

// Mapster
//builder.Services.AddMapster();
var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(typeAdapterConfig);
//builder.Services.AddScoped<IMapper, ServiceMapper>();

// SurrealDB
var surrealDbConfig = builder.Configuration.GetSection("SurrealDbSettings");
builder.Services.AddSurreal(x => x.FromConnectionString(surrealDbConfig["ConnectionString"]), ServiceLifetime.Scoped);
//builder.Services.AddScoped<SurrealDbProviderFactoryBase, SurrealDbProviderFactory>();

// Register SurrealDb Providers
//builder.Services.AddScoped<ISurrealDbProvider<CreditWallet, WalletResponseDTO>, SurrealDbProvider<CreditWallet, WalletResponseDTO>>();
//builder.Services.AddScoped<ISurrealDbProvider<Book, BookResponseDTO>, SurrealDbProvider<Book, BookResponseDTO>>();

//ตั้งค่าการเชื่อมต่อ SurrealDB (DB Provider) 
// 3. เรียกใช้ AddSurreal ด้วยพารามิเตอร์เดียวตามปกติ
//builder.Services.AddSurreal(config => { config.WithEndpoint("http://localhost:8000"); 
//    config.WithNamespace("test"); 
//    config.WithDatabase("test"); 
//    config.WithUsername("db_user"); 
//    config.WithPassword("db_pass"); }); 

//ลงทะเบียน Factory เป็น Singleton (ให้มีแค่โรงงานเดียว)
builder.Services.AddSingleton<ISurrealDbProviderFactory, SurrealDbProviderFactory>();

//ลงทะเบียน Generic DbProvider เป็น Scoped
builder.Services.AddScoped(typeof(IDbProvider<,>), typeof(DbProvider<,>)); 

//ลงทะเบียน Penalty Strategies และ Service หลัก
builder.Services.AddScoped<IDailyPenalty, DailyPenalty>(); 
builder.Services.AddScoped<IFixedPenalty, FixedPenalty>(); 
builder.Services.AddScoped<IPercentagePenalty, PercentagePenalty>(); 
builder.Services.AddScoped<Penalty>(); 
//########################### (สิ้นสุด) ส่วนที่เต้เพิ่มเข้ามา ###########################

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
