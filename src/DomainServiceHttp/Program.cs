using DomainGateway.ServiceDiscovery.Client.Configuration;
using DomainGateway.ServiceDiscovery.Client.Utils;
using Microsoft.AspNetCore.Mvc;

const string defaultFromCurrency = "USD";

var builder = WebApplication.CreateBuilder(args);

var serviceDiscoConfiguration = builder.Configuration.GetSection("ServiceDiscovery").Get<ServiceDiscoveryConfiguration>()!;
builder.Services.AddServiceDiscoveryClientWithRegistry(serviceDiscoConfiguration);

var app = builder.Build();


app.MapGet("/exchangerate", async ([FromQuery] string? fromCurrency = null) =>
	{
		var url = "https://open.er-api.com/v6/latest/" + (fromCurrency ?? defaultFromCurrency);
		using var client = new HttpClient();
		var response = await client.GetFromJsonAsync<ExchangeRateModel>(url);
		return response;
	})
	.WithName("get-exchange-rates");

app.MapGet("/heath", () => Results.Ok(new { Healthy = true })).WithName("health-indicator");

await app.RunAsync();

record ExchangeRateModel(Dictionary<string, decimal> Rates);
