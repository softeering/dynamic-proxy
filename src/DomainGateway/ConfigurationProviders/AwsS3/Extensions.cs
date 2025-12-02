using DomainGateway.Configurations;
using Amazon;
using Amazon.S3;
using DomainGateway.Contracts;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.AwsS3;

public static class Extensions
{
	public static IServiceCollection AddAwsS3Provider(this IServiceCollection services, AwsS3RepositorySetup configuration)
	{
		services.AddSingleton(configuration);
		services.AddSingleton<IAmazonS3>(sp =>
		{
			var clientConfig = new AmazonS3Config
			{
				ClientAppId = "DomainGateway",
				RegionEndpoint = RegionEndpoint.GetBySystemName(configuration.Region)
			};
			return new AmazonS3Client(clientConfig);
		});
		
		services.AddSingleton<AwsS3ConfigurationProvider>();
		services.AddSingleton<IGatewayConfigurationProvider>(sp => sp.GetRequiredService<AwsS3ConfigurationProvider>());
		services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<AwsS3ConfigurationProvider>());
		
		services.AddHostedService<AwsS3ProxyConfigurationSyncJob>();
		return services;
	}
}
