var builder = DistributedApplication.CreateBuilder(args);

var localStack = builder.AddContainer("localstack", "localstack/localstack", "latest");

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.DomainGateway>("domain-gateway")
	.WithReference(redis);

await builder.Build().RunAsync();
