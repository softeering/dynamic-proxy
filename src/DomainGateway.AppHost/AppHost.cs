var builder = DistributedApplication.CreateBuilder(args);

// Domain services
builder.AddProject<Projects.DomainServiceA>("weather-forecast");
builder.AddProject<Projects.DomainServiceB>("foreign-exchange");

// Domain Gateway
builder.AddProject<Projects.DomainGateway>("domain-gateway");

await builder.Build().RunAsync();
