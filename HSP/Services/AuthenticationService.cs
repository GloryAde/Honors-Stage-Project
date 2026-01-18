using HSP.Data;
using HSP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HSP.Services
{
    public class AuthenticationService
    {
        private readonly HspDbContext _context;
        private readonly CustomAuthenticationStateProvider _authStateProvider;

        public AuthenticationService(HspDbContext context, CustomAuthenticationStateProvider authStateProvider)
        {
            _context = context;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResult> RegisterAsync(string fullName, string email, string password, string role = "Student")
        {
            try
            {
                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (existingUser != null)
                {
                    return new AuthResult { Success = false, Message = "Email already exists." };
                }

                // Validate password
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    return new AuthResult { Success = false, Message = "Password must be at least 6 characters long." };
                }

                // Create new user with role
                var user = new HSP.Models.User
                {
                    FullName = fullName.Trim(),
                    Email = email.Trim().ToLower(),
                    PasswordHash = HashPassword(password),
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new AuthResult { Success = true, Message = $"User '{fullName}' registered successfully!" };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = $"Registration failed: {ex.Message}" };
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    return new AuthResult { Success = false, Message = "Invalid email or password." };
                }

                if (!user.IsActive)
                {
                    return new AuthResult { Success = false, Message = "Account is deactivated. Please contact an administrator." };
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return new AuthResult { Success = false, Message = "Invalid email or password." };
                }

                // Set authentication state
                await _authStateProvider.MarkUserAsAuthenticated(user);

                return new AuthResult 
                { 
                    Success = true, 
                    Message = "Login successful!", 
                    User = user 
                };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = $"Login failed: {ex.Message}" };
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public HSP.Models.User? User { get; set; }
    }
}