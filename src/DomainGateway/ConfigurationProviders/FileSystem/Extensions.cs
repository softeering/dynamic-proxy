using DomainGateway.Contracts;
using DomainGateway.Models;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.FileSystem;

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
