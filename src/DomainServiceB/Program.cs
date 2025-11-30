using Microsoft.AspNetCore.Mvc;

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

app.MapGet("/", ([FromQuery] string? fromCurrency = null) =>
	{
		var url = "https://open.er-api.com/v6/latest/" + (fromCurrency ?? "USD");
		using var client = new HttpClient();
		var response = client.GetStringAsync(url).Result;
		return response;
	})
	.WithName("GetWeatherForecast");

await app.RunAsync();
