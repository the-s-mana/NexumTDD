using Mapster;
using MapsterMapper;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Nexum.Server.DAC;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Models.Interest;
using Nexum.Server.Services;
using Nexum.Server.Services.Penalty;
using Nexum.Server.Utils;
using System.Reflection;
using static Nexum.Server.Data.IDbProviderFactory;

var builder = WebApplication.CreateBuilder(args);

// 1. เพิ่ม Service ของ App Insights
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

// 2. (สำคัญ!) ปิด Sampling เพื่อให้ Log Fraud เข้า 100%
builder.Services.Configure<ApplicationInsightsServiceOptions>(options =>
{
    options.EnableAdaptiveSampling = false;
    options.EnableEventCounterCollectionModule = false;
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DAC services
builder.Services.AddScoped<IPenaltyPoliciesDAC, PenaltyPoliciesDAC>();
builder.Services.AddScoped<ICreditWalletDAC, CreditWalletDAC>();
builder.Services.AddScoped<IProductContactDAC, ProductContactDAC>();
builder.Services.AddScoped<IProductContactDAC, ProductContactDAC>();
builder.Services.AddScoped<IAccumulatedInterestDAC, AccumulatedInterestDAC>();
builder.Services.AddScoped<IInterestTransactionDAC, InterestTransactionDAC>();

// Register Service services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IInterestService, InterestService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IContactService, ContactService>();


builder.Services.AddScoped<IPenalty, Penalty>();
builder.Services.AddScoped<IDailyPenalty, DailyPenalty>();
builder.Services.AddScoped<IPercentagePenalty, PercentagePenalty>();
builder.Services.AddScoped<IPenaltyPolicies, PenaltyPolicies>();
builder.Services.AddScoped<IFixedPenalty, FixedPenalty>();

// Register Utils
builder.Services.AddScoped<IDateTimeUtils, DateTimeUtils>();

// Mapster
builder.Services.AddMapster();
var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(typeAdapterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// SurrealDB
var surrealDbConfig = builder.Configuration.GetSection("SurrealDbSettings");
builder.Services.AddSurreal(x => x.FromConnectionString(surrealDbConfig["ConnectionString"]), ServiceLifetime.Scoped);
builder.Services.AddScoped<SurrealDbProviderFactoryBase, SurrealDbProviderFactory>();

// Register SurrealDb Providers
builder.Services.AddScoped<ISurrealDbProvider<Book, BookResponseDTO>, SurrealDbProvider<Book, BookResponseDTO>>();
builder.Services.AddScoped<ISurrealDbProvider<CreditWallet, WalletResponseDTO>, SurrealDbProvider<CreditWallet, WalletResponseDTO>>();
builder.Services.AddScoped<ISurrealDbProvider<ProductContact, ContactResponseDTO>, SurrealDbProvider<ProductContact, ContactResponseDTO>>();
builder.Services.AddScoped<ISurrealDbProvider<AccumulatedInterest, AccumulatedInterestResponseDTO>, SurrealDbProvider<AccumulatedInterest, AccumulatedInterestResponseDTO>>();
builder.Services.AddScoped<ISurrealDbProvider<InterestTransaction, CreateInterestTransactionDTO>, SurrealDbProvider<InterestTransaction, CreateInterestTransactionDTO>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
