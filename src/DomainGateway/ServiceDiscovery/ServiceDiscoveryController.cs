using DomainGateway.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainGateway.ServiceDiscovery;

[Authorize(Policy = ClientIdHeaderRequirementHandler.PolicyName)]
[ApiController]
[Route("api/[controller]")]
public class ServiceDiscoveryController(ILogger<ServiceDiscoveryController> logger, IServiceDiscoveryInstancesRepository repository) : ControllerBase
{
	[HttpGet("{serviceName}/instances")]
	public async Task<IActionResult> GetServiceInstances(string serviceName, CancellationToken cancellationToken)
	{
		var instances = await repository.GetRegisteredInstancesAsync(serviceName, cancellationToken);
		return Ok(instances.Select(i => i.ToServiceInstance()));
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
		return Ok();
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
