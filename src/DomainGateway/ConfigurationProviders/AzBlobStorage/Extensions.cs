using DomainGateway.Configurations;

namespace DomainGateway.ConfigurationProviders.AzBlobStorage;

public static class Extensions
{
	public static IServiceCollection AddAzBlobStorageProvider(this IServiceCollection services, AzBlobStorageRepositorySetup configuration)
	{
		services.AddSingleton(configuration);
		
		return services;
	}
}
