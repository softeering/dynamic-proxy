using DomainGateway.Configurations;

namespace DomainGateway.ConfigurationProviders.FileSystem;

using Contracts;
using Yarp.ReverseProxy.Configuration;

public static class Extensions
{
    public static IServiceCollection AddFileSystemProvider(this IServiceCollection services, FileSystemRepositorySetup configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IGatewayConfigurationProvider, FileSystemGatewayConfigurationProvider>();
        services.AddSingleton<IProxyConfigProvider, FileSystemGatewayConfigurationProvider>();
        services.AddHostedService<FileSystemGatewayConfigurationSyncJob>();
        return services;
    }
}
