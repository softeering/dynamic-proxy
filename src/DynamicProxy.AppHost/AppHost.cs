var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.DynamicProxy>("dynamicproxy")
	.WithReference(redis);

await builder.Build().RunAsync();
