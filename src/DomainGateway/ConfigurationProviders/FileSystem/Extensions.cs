using DomainGateway.Configurations;
using DomainGateway.Contracts;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public static class Extensions
{
    public static IServiceCollection AddFileSystemProvider(this IServiceCollection services, FileSystemRepositorySetup configuration)
    {
        services.AddSingleton(configuration);
		services.AddSingleton<IGatewayConfigurationProvider, FileSystemGatewayConfigurationProvider>();
        
        return services;
    }
}
