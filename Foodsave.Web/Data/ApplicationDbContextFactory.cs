using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Foodsave.Web.Data
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environment =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                "Development";
            var currentDirectory = Directory.GetCurrentDirectory();
            var configurationDirectory = File.Exists(
                Path.Combine(currentDirectory, "appsettings.json"))
                ? currentDirectory
                : Path.Combine(currentDirectory, "Foodsave.Web");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(configurationDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(DatabaseConnectionStringResolver.Resolve(configuration))
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
