var builder = DistributedApplication.CreateBuilder(args);

// Domain services
builder.AddProject<Projects.DomainServiceHttp>("foreign-exchange-http");
builder.AddProject<Projects.DomainServiceGrpc>("foreign-exchange-grpc");

// Domain Gateway
builder.AddProject<Projects.DomainGateway>("domain-gateway");

await builder.Build().RunAsync();
