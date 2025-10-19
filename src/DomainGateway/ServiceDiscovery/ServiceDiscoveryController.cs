using Microsoft.AspNetCore.Mvc;

namespace DomainGateway.ServiceDiscovery;

[ApiController]
[Route("api/[controller]")]
public class ServiceDiscoveryController(ILogger<ServiceDiscoveryController> logger, IServiceDiscoveryInstancesRepository repository) : ControllerBase
{
	
}
