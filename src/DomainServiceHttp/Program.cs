using System.Net;
using DomainGateway.ServiceDiscovery.Client.Configuration;
using DomainGateway.ServiceDiscovery.Client.Utils;
using DomainServiceHttp;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var serviceDiscoConfiguration = builder.Configuration.GetSection("ServiceDiscovery").Get<ServiceDiscoveryConfiguration>()!;
builder.Services.AddServiceDiscoveryClientWithRegistry(serviceDiscoConfiguration, HostHelper.GetLocalIPv4()?.ToString());

var address = HostHelper.GetLocalIPv4();
if (address is not null && Equals(address, IPAddress.Loopback))
	builder.Services.AddSingleton<IExchangeRateRepository, OfflineExchangeRateRepository>();
else
	builder.Services.AddSingleton<IExchangeRateRepository, OnlineExchangeRateRepository>();

var app = builder.Build();

app.MapGet("/exchangerate", async (IExchangeRateRepository exchangeRateRepository, [FromQuery] string? fromCurrency = null) =>
	{
		var result = await exchangeRateRepository.GetExchangeRate(fromCurrency);
		return result;
	})
	.WithName("get-exchange-rates");

app.MapPost("/exchangerate", async (ILogger<Program> logger, ExchangeRateUpdateModel model) =>
	{
		await Task.Delay(200); // Simulate some processing delay
		// store the update
		logger.LogInformation("Exchange rate updated: {FromCurrency} to {ToCurrency} = {Rate}", model.FromCurrency, model.ToCurrency, model.Rate);
		return Results.Ok();
	})
	.WithName("update-exchange-rates");

app.MapGet("/health", () => Results.Ok(new { Healthy = true })).WithName("health-indicator");

await app.RunAsync();
