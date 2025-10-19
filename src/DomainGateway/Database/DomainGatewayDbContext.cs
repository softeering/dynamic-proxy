namespace DomainGateway.Database;

using ServiceDiscovery;
using Microsoft.EntityFrameworkCore;

public class DomainGatewayDbContext(DbContextOptions<DomainGatewayDbContext> options) : DbContext(options)
{
	public DbSet<ServiceInstance> Instances { get; init; }

	public static void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
	{
		var schemeEnd = connectionString.IndexOf("://", StringComparison.Ordinal);
		if (schemeEnd < 0)
			throw new ArgumentException("Invalid connection string format (Supported format is db:provider://...)");

		// options.UseApplicationServiceProvider(builder.Services.BuildServiceProvider());

		var scheme = connectionString.Substring(0, schemeEnd).ToLowerInvariant();
		var actualConnStr = connectionString.Substring(schemeEnd + 3);

		switch (scheme)
		{
			case "db:postgres":
				optionsBuilder.UseNpgsql(actualConnStr);
				break;
			case "db:sqlserver":
				optionsBuilder.UseSqlServer(actualConnStr);
				break;
			case "db:mysql":
				optionsBuilder.UseMySQL(actualConnStr);
				break;
			default:
				throw new NotSupportedException($"Scheme '{scheme}' is not supported (Supported schemes are: 'db:postgres', 'db:sqlserver', 'db:mysql').");
		}
	}
}
