using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SoftEEring.Core.Helpers;

namespace DomainGateway.Infrastructure;

public static class Extensions
{
	public static IServiceScope GetScopedService<TService>(this IServiceProvider serviceFactory, out TService service)
		where TService : notnull
	{
		var scope = serviceFactory.CreateScope();
		service = scope.ServiceProvider.GetRequiredService<TService>();
		return scope;
	}

	public static IServiceScope GetScopedService<TService1, TService2>(this IServiceProvider serviceFactory, out TService1 service1, out TService2 service2)
		where TService1 : notnull
		where TService2 : notnull
	{
		var scope = serviceFactory.CreateScope();
		service1 = scope.ServiceProvider.GetRequiredService<TService1>();
		service2 = scope.ServiceProvider.GetRequiredService<TService2>();
		return scope;
	}

	public static IServiceScope GetScopedService<TService1, TService2, TService3>(
		this IServiceProvider serviceFactory,
		out TService1 service1,
		out TService2 service2,
		out TService3 service3)
		where TService1 : notnull
		where TService2 : notnull
		where TService3 : notnull
	{
		var scope = serviceFactory.CreateScope();
		service1 = scope.ServiceProvider.GetRequiredService<TService1>();
		service2 = scope.ServiceProvider.GetRequiredService<TService2>();
		service3 = scope.ServiceProvider.GetRequiredService<TService3>();
		return scope;
	}

	public static void AttachAs<TEntity>(this DbSet<TEntity> dbSet, TEntity entity, EntityState state) where TEntity : class
	{
		dbSet.Attach(entity).State = state;
	}

	public static void ThrowIfNotValid(this ModelStateDictionary state, string? message = null)
	{
		if (state.IsValid)
			return;

		if (message is not null)
			throw new ArgumentException(message);

		throw new ArgumentException("Invalid model: " + state);
	}
}
