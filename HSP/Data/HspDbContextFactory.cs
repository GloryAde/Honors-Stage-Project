using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HSP.Data
{
    public class HspDbContextFactory : IDesignTimeDbContextFactory<HspDbContext>
    {
        public HspDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<HspDbContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("HspDatabase"));

            return new HspDbContext(optionsBuilder.Options);
        }
    }
}
