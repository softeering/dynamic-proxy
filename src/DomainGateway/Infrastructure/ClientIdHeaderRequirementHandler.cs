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

public class ClientIdHeaderRequirementHandler(IGatewayConfigurationProvider configurationProvider) : AuthorizationHandler<ClientIdHeaderRequirement>
{
	public const string PolicyName = "ClientIdHeaderPolicy";
	
	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClientIdHeaderRequirement requirement)
	{
		// var httpContext = (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)?.HttpContext;
		if (context.Resource is DefaultHttpContext httpContext && httpContext.Request.Headers.TryGetValue(requirement.HeaderName, out var value) && value.Count > 0)
		{
			var allowedClients = configurationProvider.GetServiceDiscoveryConfiguration().AllowedClients;
			if (requirement.IsAllowed(value) && allowedClients.Contains(value!))
			{
				context.Succeed(requirement);
			}
		}

		return Task.CompletedTask;
	}
}
