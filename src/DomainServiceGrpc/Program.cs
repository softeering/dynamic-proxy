using System.Net;
using DomainGateway.ServiceDiscovery.Client.Configuration;
using DomainGateway.ServiceDiscovery.Client.Utils;
using DomainServiceGrpc.Services;
using DomainServiceHttp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var serviceDiscoConfiguration = builder.Configuration.GetSection("ServiceDiscovery").Get<ServiceDiscoveryConfiguration>()!;
builder.Services.AddServiceDiscoveryClientWithRegistry(serviceDiscoConfiguration, HostHelper.GetLocalIPv4()?.ToString());

var address = HostHelper.GetLocalIPv4();
if (address is not null && Equals(address, IPAddress.Loopback))
	builder.Services.AddSingleton<IExchangeRateRepository, OfflineExchangeRateRepository>();
else
	builder.Services.AddSingleton<IExchangeRateRepository, OnlineExchangeRateRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ExchangeRateService>();
app.MapGrpcReflectionService();
app.MapGet("/", () => new { Error = "Communication with gRPC endpoints must be made through a gRPC client" });
app.MapGet("/health", () => Results.Ok(new { Healthy = true })).WithName("health-indicator");

await app.RunAsync();
