using DynamicProxy.Models;

namespace DynamicProxy.ConfigurationProviders.AwsS3;

public static class Extensions
{
    public static IServiceCollection AddS3Yarp(this IServiceCollection services, AwsS3Repository configuration)
    {
        // services.AddAWSService<IAmazonS3>(new AWSOptions { Region = RegionEndpoint.USEast1 });
        // services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<S3ProxyConfigProvider>());
        services.AddHostedService<AwsS3ConfigurationSyncJob>();
        return services;
    }
}
