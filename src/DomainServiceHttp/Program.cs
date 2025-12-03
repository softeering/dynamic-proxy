using Microsoft.AspNetCore.Mvc;

const string DEFAULT_FROM_CURRENCY = "USD";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", async ([FromQuery] string? fromCurrency = null) =>
	{
		var url = "https://open.er-api.com/v6/latest/" + (fromCurrency ?? DEFAULT_FROM_CURRENCY);
		using var client = new HttpClient();
		var response = await client.GetFromJsonAsync<ExchangeRateModel>(url);
		return response;
	})
	.WithName("get-exchange-rates");

await app.RunAsync();

record ExchangeRateModel(Dictionary<string, string> Rates);
