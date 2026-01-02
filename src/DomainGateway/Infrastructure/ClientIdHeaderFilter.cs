using DomainGateway.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DomainGateway.Infrastructure;

public sealed class ClientIdHeaderFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (!context.HttpContext.Request.Headers.TryGetValue(RateLimiterConfiguration.ClientIdHeaderName, out var value) ||
		    string.IsNullOrWhiteSpace(value))
		{
			context.Result = new BadRequestObjectResult($"Missing required header '{RateLimiterConfiguration.ClientIdHeaderName}'.");
			return;
		}

		await next();
	}
}
