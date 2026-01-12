using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HSP.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly HspDbContext _context;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage, HspDbContext context)
        {
            _sessionStorage = sessionStorage;
            _context = context;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<int>("userId");
                
                if (!userSessionStorageResult.Success || userSessionStorageResult.Value == 0)
                {
                    return new AuthenticationState(_anonymous);
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userSessionStorageResult.Value && u.IsActive);

                if (user == null)
                {
                    return new AuthenticationState(_anonymous);
                }

                var claimsPrincipal = CreateClaimsPrincipal(user);
                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task MarkUserAsAuthenticated(User user)
        {
            await _sessionStorage.SetAsync("userId", user.UserId);
            
            var claimsPrincipal = CreateClaimsPrincipal(user);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _sessionStorage.DeleteAsync("userId");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<int>("userId");
                
                if (!userSessionStorageResult.Success || userSessionStorageResult.Value == 0)
                {
                    return null;
                }

                return await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userSessionStorageResult.Value && u.IsActive);
            }
            catch
            {
                return null;
            }
        }

        private ClaimsPrincipal CreateClaimsPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),      
            };

            var identity = new ClaimsIdentity(claims, "CustomAuthentication");
            return new ClaimsPrincipal(identity);
        }
    }
}