var builder = DistributedApplication.CreateBuilder(args);

// var localStack = builder.AddContainer("localstack", "localstack/localstack", "latest");

var redis = builder.AddRedis("redis")
	.WithContainerName("dg-redis"); // .WithImageTag("");

var dbUserName = builder.AddParameter("dbusername", secret: true);
var dbPassword = builder.AddParameter("dbpassword", secret: true);

var postgresdb = builder.AddPostgres("postgres", dbUserName, dbPassword)
	.WithContainerName("dg-postgres")
	.WithImageTag("17.6-alpine")
	.AddDatabase("postgresdb", databaseName: "domaingatewaydb");

builder.AddProject<Projects.DomainGateway>("domain-gateway")
	.WithReference(redis)
	.WithEnvironment("POSTGRES_USER", dbUserName)
	.WithEnvironment("POSTGRES_PASSWORD", dbPassword)
	.WithReference(postgresdb)
	.WaitFor(postgresdb);

await builder.Build().RunAsync();
