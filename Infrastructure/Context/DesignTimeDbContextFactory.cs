using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context
{
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MatrixDbContext>
	{
		public MatrixDbContext CreateDbContext(string[] args)
		{
			// This ensures we always start from the startup project directory
			var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

			var configuration = new ConfigurationBuilder()
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.Build();

			var connectionString = configuration.GetConnectionString("DefaultConnection");

			if (string.IsNullOrWhiteSpace(connectionString))
				throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

			var optionsBuilder = new DbContextOptionsBuilder<MatrixDbContext>();
			optionsBuilder.UseNpgsql(connectionString);

			return new MatrixDbContext(optionsBuilder.Options);
		}
	}
}
