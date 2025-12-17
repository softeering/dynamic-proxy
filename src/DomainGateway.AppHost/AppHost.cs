var builder = DistributedApplication.CreateBuilder(args);

// Domain Gateway
var gateway = builder.AddProject<Projects.DomainGateway>("domain-gateway");

// Domain services
builder.AddProject<Projects.DomainServiceHttp>("foreign-exchange-http").WaitFor(gateway);
builder.AddProject<Projects.DomainServiceGrpc>("foreign-exchange-grpc").WaitFor(gateway);

await builder.Build().RunAsync();
