using HSP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HSP.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDefaultAdminAsync(HspDbContext context)
        {
            // Check if any users exist
            if (await context.Users.AnyAsync())
            {
                return; // Database already has users, skip seeding
            }

            // Create default administrator account
            var defaultAdmin = new User
            {
                FullName = "System Administrator",
                Email = "admin@hsp.com",
                PasswordHash = HashPassword("Admin123!"), // Change the password for later
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(defaultAdmin);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Default administrator account created:");
            Console.WriteLine("   Email: admin@hsp.com");
            Console.WriteLine("   Password: Admin123!");
            Console.WriteLine("   ⚠️ Please change the password after first login!");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}