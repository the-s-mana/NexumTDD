using Nexum.Server.Data;
using Nexum.Server.DAC;
using Nexum.Server.Services;
using Nexum.Server.Services.Penalty;
using Nexum.Server.Utils;
using SurrealDb.Net;

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

// Register DateTimeUtils
builder.Services.AddScoped<IDateTimeUtils, DateTimeUtils>();

builder.Services.AddSingleton<ISurrealDbProvider, SurrealDbProvider>();
builder.Services.AddTransient<IBookService, BookService>();


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
