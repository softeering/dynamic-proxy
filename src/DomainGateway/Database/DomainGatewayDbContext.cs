namespace DomainGateway.Database;

using ServiceDiscovery;
using Microsoft.EntityFrameworkCore;

/**
 * EF migration
 *
 * run this command i the terminal
 *		dotnet ef migrations add InitialCreate --output-dir Database/Migrations
 * Example
 *		dotnet ef migrations add InitialCreate --context DomainGatewayDbContext --project DomainGateway --startup-project DomainGateway --output-dir Database/Migrations
 *
 * Reset migrations
 *		dotnet ef migrations remove
 */
public class DomainGatewayDbContext(DbContextOptions<DomainGatewayDbContext> options, IConfiguration configuration) : DbContext(options)
{
	private const string PostgresScheme = "db:postgres";
	private const string SqlServerScheme = "db:sqlserver";
	private const string MySqlScheme = "db:mysql";
	private const string SqliteScheme = "db:sqlite";

	public DbSet<ServiceInstanceEntity> Instances { get; init; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")!;
		var schemeEnd = connectionString.IndexOf("://", StringComparison.Ordinal);
		if (schemeEnd < 0)
			throw new ArgumentException("Invalid connection string format (Supported format is db:provider://...)");

		// options.UseApplicationServiceProvider(builder.Services.BuildServiceProvider());

		var scheme = connectionString.Substring(0, schemeEnd).ToLowerInvariant();
		var actualConnStr = connectionString.Substring(schemeEnd + 3);

		switch (scheme)
		{
			case PostgresScheme:
				optionsBuilder.UseNpgsql(actualConnStr);
				break;
			case SqlServerScheme:
				optionsBuilder.UseSqlServer(actualConnStr);
				break;
			case MySqlScheme:
				optionsBuilder.UseMySQL(actualConnStr);
				break;
			case SqliteScheme:
				optionsBuilder.UseSqlite(actualConnStr);
				break;
			default:
				throw new NotSupportedException($"Scheme '{scheme}' is not supported (Supported schemes are: 'db:postgres', 'db:sqlserver', 'db:mysql').");
		}

		base.OnConfiguring(optionsBuilder);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ServiceInstanceEntity>()
			.HasKey(e => new { e.ServiceName, e.InstanceId });

		modelBuilder.Entity<ServiceInstanceEntity>()
			.HasIndex(e => e.ServiceName);
	}
}
