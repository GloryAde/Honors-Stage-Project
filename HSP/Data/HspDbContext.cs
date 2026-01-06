using Microsoft.EntityFrameworkCore;
using HSP.Models;

namespace HSP.Data
{
    public class HspDbContext : DbContext
    {
        public HspDbContext(DbContextOptions<HspDbContext> options) : base(options) { }

        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}
