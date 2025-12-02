using DomainGateway.Configurations;
using DomainGateway.Contracts;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public static class Extensions
{
    public static IServiceCollection AddFileSystemProvider(this IServiceCollection services, FileSystemRepositorySetup configuration)
    {
        services.AddSingleton(configuration);
        
		services.AddSingleton<FileSystemGatewayConfigurationProvider>();
        services.AddSingleton<IGatewayConfigurationProvider>(sp => sp.GetRequiredService<FileSystemGatewayConfigurationProvider>());
        services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<FileSystemGatewayConfigurationProvider>());
        
        services.AddHostedService<FileSystemGatewayConfigurationSyncJob>();
        return services;
    }
}
