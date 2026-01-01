using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DomainGateway.Client.Core.Models;
using DomainGateway.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SoftEEring.Core.Helpers;

namespace DomainGateway.Tests;

[TestClass]
public class ServiceDiscoveryControllerTests : IDisposable
{
    private readonly ServiceDiscoveryWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ServiceDiscoveryControllerTests()
    {
        this._factory = new ServiceDiscoveryWebApplicationFactory();
        this._client = _factory.CreateClient();
        this._client.DefaultRequestHeaders.Add("Client-Id", "test-client");
    }

    [TestCleanup]
    public async Task CleanupTestData()
    {
        await using var scope = this._factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DomainGatewayDbContext>();
        dbContext.Instances.RemoveRange(dbContext.Instances);
        await dbContext.SaveChangesAsync();
    }

    [TestMethod]
    public async Task RegisterInstance_PersistsAndIsRetrievable()
    {
        // Arrange
        const string serviceName = "exchange-rate-service";
        var instance = new ServiceInstance(serviceName, "instance-1", "1.0.0", "127.0.0.1", 8085, new Dictionary<string, string>());

        // Act - get all instances
        var allInstancesResponse = await this._client.GetAsync("api/ServiceDiscovery/instances");
        // Assert - should be empty initially
        Assert.AreEqual(HttpStatusCode.OK, allInstancesResponse.StatusCode, "Instance should be retrievable after registration.");
        var emptyListResponse = await this.ReadJsonResponseAsync<ServiceInstance[]>(allInstancesResponse);
        Assert.IsNotNull(emptyListResponse);
        Assert.HasCount(0, emptyListResponse);
        
        // Act
        var registerResponse = await this._client.PostAsJsonAsync($"api/ServiceDiscovery/{serviceName}/register", instance);
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, registerResponse.StatusCode, "Register endpoint should return 201 Created.");

        // Act - get all instances
        var allInstancesPostRegistrationResponse = await this._client.GetAsync("api/ServiceDiscovery/instances");
        // Assert - should be empty initially
        Assert.AreEqual(HttpStatusCode.OK, allInstancesPostRegistrationResponse.StatusCode, "Instance should be retrievable after registration.");
        var allInstancesListResponse = await this.ReadJsonResponseAsync<ServiceInstance[]>(allInstancesPostRegistrationResponse);
        Assert.IsNotNull(allInstancesListResponse);
        Assert.HasCount(1, allInstancesListResponse);
        
        // Act
        var serviceInstancesResponse = await this._client.GetAsync($"api/ServiceDiscovery/{serviceName}/instances");
        // Assert - should contain the registered instance
        Assert.AreEqual(HttpStatusCode.OK, serviceInstancesResponse.StatusCode, "Instances for service should be retrievable after registration.");
        var serviceInstancesListResponse = await this.ReadJsonResponseAsync<ServiceInstance[]>(serviceInstancesResponse);
        Assert.IsNotNull(serviceInstancesListResponse);
        Assert.HasCount(1, serviceInstancesListResponse);
        
        // Act
        var persistedInstanceResponse = await this._client.GetAsync($"api/ServiceDiscovery/{serviceName}/instances/{instance.InstanceId}");
        var persistedInstance = await this.ReadJsonResponseAsync<ServiceInstance>(persistedInstanceResponse);
        Assert.IsNotNull(persistedInstance);
        Assert.AreEqual(instance.InstanceId, persistedInstance!.InstanceId);
        Assert.AreEqual(instance.ServiceName, persistedInstance.ServiceName);
    }

    private async Task<T?> ReadJsonResponseAsync<T>(HttpResponseMessage response)
    {
	    var content = await response.Content.ReadAsStringAsync();
	    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public void Dispose()
    {
	    this._client.Dispose();
        this._factory.Dispose();
    }

    private sealed class ServiceDiscoveryWebApplicationFactory : WebApplicationFactory<Program>
    {
	    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();
	    
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace DbContext with in-memory provider for isolation
                services.SingleOrDefault(d => d.ServiceType == typeof(DomainGatewayDbContext))?.Let(services.Remove);
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DomainGatewayDbContext>))?.Let(services.Remove);
                
                services.AddDbContext<DomainGatewayDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ServiceDiscoveryTests", DatabaseRoot);
                });
            });
        }
    }
}
