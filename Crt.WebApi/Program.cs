#pragma warning disable SA1200
using Crt;
using Crt.Core.Models;
using Crt.Provider.CurrencyLayerProvider;
#pragma warning restore SA1200

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddProviderCurrencyLayer()
    .AddCrt();

var configuration = builder.Configuration;
builder.Services.Configure<ProviderSettings>(configuration.GetSection(nameof(ProviderSettings)));

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();