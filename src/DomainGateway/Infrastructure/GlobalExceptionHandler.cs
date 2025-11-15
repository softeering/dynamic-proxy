using Microsoft.AspNetCore.Http.Extensions;
using System.Security.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DomainGateway.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
	public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		logger.LogError(exception, "API controller failure for url {Url}", httpContext.Request.GetDisplayUrl());

		var problemDetails = new ProblemDetails
		{
			Status = exception switch
			{
				InvalidCredentialException => StatusCodes.Status403Forbidden,
				ArgumentException => StatusCodes.Status400BadRequest,
				_ => StatusCodes.Status500InternalServerError
			},
			Title = "An error occurred",
			Type = exception.GetType().Name,
			Detail = exception.Message
		};

		return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			Exception = exception,
			HttpContext = httpContext,
			ProblemDetails = problemDetails
		});
	}
}
