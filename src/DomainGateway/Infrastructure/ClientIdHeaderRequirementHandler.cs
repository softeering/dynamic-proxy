using DomainGateway.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace DomainGateway.Infrastructure;

public record ClientIdHeaderRequirement(string HeaderName) : IAuthorizationRequirement
{
	public bool IsAllowed(string? clientId)
	{
		return !string.IsNullOrWhiteSpace(clientId);
	}
}

public class ClientIdHeaderRequirementHandler(IGatewayConfigurationService gatewayConfigurationService, IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<ClientIdHeaderRequirement>
{
	// public const string PolicyName = "ClientIdHeaderPolicy";
	
	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClientIdHeaderRequirement requirement)
	{
		var httpContext = context.Resource as HttpContext ?? httpContextAccessor.HttpContext;
		if (httpContext is not null && httpContext.Request.Headers.TryGetValue(requirement.HeaderName, out var value) && value.Count > 0)
		{
			var allowedClients = gatewayConfigurationService.GetServiceDiscoveryConfiguration().AllowedClients;
			if (requirement.IsAllowed(value) && (allowedClients is null || allowedClients.Contains(value!)))
			{
				context.Succeed(requirement);
			}
		}

		return Task.CompletedTask;
	}
}
