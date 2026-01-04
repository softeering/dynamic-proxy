using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var baseAddress = builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(_ =>
{
	var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
	client.DefaultRequestHeaders.Add("Client-Id", "DomainGateway-Admin-Portal");
	return client;
});

await builder.Build().RunAsync();
