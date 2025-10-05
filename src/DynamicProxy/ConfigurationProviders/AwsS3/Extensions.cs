using Amazon;
using Amazon.S3;
using DynamicProxy.Models;
using DynamicProxy.Services;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;

namespace DynamicProxy.ConfigurationProviders.AwsS3;

public static class Extensions
{
    public static IServiceCollection AddAwsS3Provider(this IServiceCollection services, AwsS3RepositorySetup configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var clientConfig = new AmazonS3Config
            {
                ClientAppId = "DynamicProxy",
                RegionEndpoint = RegionEndpoint.GetBySystemName(configuration.Region)
            };
            return new AmazonS3Client(clientConfig);
        });
        services.AddSingleton<IGatewayConfigurationProvider, AwsS3ConfigurationProvider>();
        services.AddSingleton<IProxyConfigProvider, AwsS3ConfigurationProvider>();
        services.AddHostedService<AwsS3ProxyConfigurationSyncJob>();
        services.AddHostedService<AwsS3ThrottlingConfigurationSyncJob>();
        return services;
    }
}