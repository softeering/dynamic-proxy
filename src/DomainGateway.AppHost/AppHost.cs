var builder = DistributedApplication.CreateBuilder(args);

// var localStack = builder.AddContainer("localstack", "localstack/localstack", "latest");

var redis = builder.AddRedis("redis")
	.WithContainerName("dg-redis"); // .WithImageTag("");

var postgresdb = builder.AddPostgres("postgres")
	.WithContainerName("dg-postgres")
	.WithImageTag("17.6-alpine")
	.AddDatabase("postgresdb", databaseName: "domaingatewaydb");

builder.AddProject<Projects.DomainGateway>("domain-gateway")
	.WithReference(redis)
	.WithReference(postgresdb)
	.WaitFor(postgresdb);

await builder.Build().RunAsync();
