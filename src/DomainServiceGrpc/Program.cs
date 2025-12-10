using DomainServiceGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ExchangeRateService>();
app.MapGrpcReflectionService();
app.MapGet("/", () => new { Error = "Communication with gRPC endpoints must be made through a gRPC client" });

await app.RunAsync();
