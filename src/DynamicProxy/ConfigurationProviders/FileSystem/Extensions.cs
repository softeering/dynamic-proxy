using DynamicProxy.Models;
using DynamicProxy.Services;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;

namespace DynamicProxy.ConfigurationProviders.FileSystem;

public static class Extensions
{
    public static IServiceCollection AddFileSystemProvider(this IServiceCollection services, FileSystemRepositorySetup configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IGatewayConfigurationProvider, FileSystemGatewayConfigurationProvider>();
        services.AddSingleton<IProxyConfigProvider, FileSystemGatewayConfigurationProvider>();
        return services;
    }
}