using DomainGateway.Client.Core.Models;
using DomainGateway.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainGateway.ServiceDiscovery;

// [Authorize(Policy = ClientIdHeaderRequirementHandler.PolicyName)]
[ServiceFilter(typeof(ClientIdHeaderFilter))]
[ApiController]
[Route("api/[controller]")]
public class ServiceDiscoveryController(ILogger<ServiceDiscoveryController> logger, IServiceDiscoveryInstancesRepository repository) : ControllerBase
{
	[HttpGet("instances")]
	public async Task<IActionResult> GetInstances(CancellationToken cancellationToken)
	{
		var instances = await repository.GetAllRegisteredInstancesAsync(cancellationToken);
		return Ok(instances);
	}

	[HttpGet("{serviceName}/instances")]
	public async Task<IActionResult> GetServiceInstances(string serviceName, CancellationToken cancellationToken)
	{
		var instances = await repository.GetRegisteredInstancesAsync(serviceName, cancellationToken);
		return Ok(instances);
	}

	[HttpGet("{serviceName}/instances/{instanceId}")]
	public async Task<IActionResult> GetServiceInstance(string serviceName, string instanceId, CancellationToken cancellationToken)
	{
		var instance = await repository.GetRegisteredInstanceAsync(serviceName, instanceId, cancellationToken);
		return instance is null ? NotFound() : Ok(instance);
	}

	[HttpDelete("{serviceName}/{instanceId}")]
	public async Task<IActionResult> DeregisterInstance(string serviceName, string instanceId, CancellationToken cancellationToken)
	{
		await repository.DeregisterInstanceAsync(serviceName, instanceId, cancellationToken);
		return Ok();
	}

	[HttpPost("{serviceName}/register")]
	public async Task<IActionResult> RegisterInstance(string serviceName, ServiceInstance model, CancellationToken cancellationToken)
	{
		ModelState.ThrowIfNotValid();

		if (!model.ServiceName.Equals(serviceName))
			throw new ArgumentException("serviceName in URL does not match serviceName in body");

		await repository.RegisterInstanceAsync(model, cancellationToken);
		return CreatedAtAction(nameof(GetServiceInstance), new { serviceName = model.ServiceName, instanceId = model.InstanceId }, null);
	}

	[HttpPut("{serviceName}/ping")]
	public async Task<IActionResult> HeartBeat(string serviceName, ServiceInstance model, CancellationToken cancellationToken)
	{
		ModelState.ThrowIfNotValid();

		if (!model.ServiceName.Equals(serviceName))
			throw new ArgumentException("serviceName in URL does not match serviceName in body");

		await repository.PingAsync(model, cancellationToken);
		return Ok();
	}
}
