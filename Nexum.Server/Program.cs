using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexum.Server.DAC;
using Nexum.Server.Infrastructures.Surreal;
using Nexum.Server.Services;
using Nexum.Server.Services.Penalty;
using SurrealDb.Net; 

var builder = WebApplication.CreateBuilder(args);

// ---------- SurrealDB (Options + Client + Factory + Provider) ----------
builder.Services.AddHttpClient();

builder.Services.AddOptions<SurrealOptions>()
    .Bind(builder.Configuration.GetSection("Surreal"))
    .Validate(o => Uri.TryCreate(o.Endpoint, UriKind.Absolute, out _), "Invalid Surreal.Endpoint")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Namespace) && !string.IsNullOrWhiteSpace(o.Database), "NS/DB required")
    .ValidateOnStart();

// main client ของ SurrealDb.Net (สร้างจาก Endpoint)
builder.Services.AddSingleton<ISurrealDbClient>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<SurrealOptions>>().Value;
    return new SurrealDbClient(opt.Endpoint);
});

builder.Services.AddTransient(typeof(IDbProvider<,>), typeof(DbProvider<,>));
builder.Services.AddSingleton<SurrealDbProviderFactoryBase, SurrealDbProviderFactory>();

// ---------- MVC + Swagger ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- DAC ----------
builder.Services.AddScoped<ICreditWalletDAC, CreditWalletDAC>();
builder.Services.AddScoped<IProductContactDAC, ProductContactDAC>();
builder.Services.AddScoped<IAccumulatedInterestDAC, AccumulatedInterestDAC>();
builder.Services.AddScoped<IInterestTransactionDAC, InterestTransactionDAC>();
builder.Services.AddScoped<IPenaltyPoliciesDAC, PenaltyPoliciesDAC>();

// ---------- Services (Business Layer) ----------
builder.Services.AddScoped<IInterestService, InterestService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IPenalty, Penalty>();
builder.Services.AddScoped<IDailyPenalty, DailyPenalty>();
builder.Services.AddScoped<IPercentagePenalty, PercentagePenalty>();
builder.Services.AddScoped<IPenaltyPolicies, PenaltyPolicies>();
builder.Services.AddScoped<IFixedPenalty, FixedPenalty>();

var app = builder.Build();

//// (ทางเลือก) ทริกเกอร์ Factory ให้ SignIn ตั้งแต่สตาร์ทแอป
//using (var scope = app.Services.CreateScope())
//{
//    _ = scope.ServiceProvider.GetRequiredService<SurrealDbProviderFactoryBase>();
//}

// ---------- Pipeline ----------
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
