using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using HSP.Services;

namespace HSP.Components.Pages
{
    public partial class Users : ComponentBase
    {
        [Inject]
        public HspDbContext Db { get; set; } = default!;

        [Inject]
        public AuthenticationService AuthService { get; set; } = default!;

        private List<User>? UserList { get; set; }
        private bool showAddForm = false;
        private UserFormModel newUser = new();
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            UserList = await Db.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        private void ShowAddUserForm()
        {
            showAddForm = true;
            newUser = new UserFormModel { Role = "Student" };
            ClearMessages();
        }

        private void CancelAddUser()
        {
            showAddForm = false;
            newUser = new();
            ClearMessages();
        }

        private async Task HandleAddUserAsync()
        {
            ClearMessages();

            try
            {
                if (string.IsNullOrWhiteSpace(newUser.Password) || newUser.Password.Length < 6)
                {
                    errorMessage = "Password must be at least 6 characters long.";
                    return;
                }

                var result = await AuthService.RegisterAsync(
                    newUser.FullName,
                    newUser.Email,
                    newUser.Password,
                    newUser.Role
                );

                if (result.Success)
                {
                    successMessage = result.Message;
                    await LoadUsersAsync();
                    await Task.Delay(1500);
                    CancelAddUser();
                }
                else
                {
                    errorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error adding user: {ex.Message}";
            }
        }

        private async Task DeactivateUserAsync(User user)
        {
            try
            {
                user.IsActive = false;
                await Db.SaveChangesAsync();
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deactivating user: {ex.Message}";
            }
        }

        private async Task ActivateUserAsync(User user)
        {
            try
            {
                user.IsActive = true;
                await Db.SaveChangesAsync();
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error activating user: {ex.Message}";
            }
        }

        private async Task DeleteUserAsync(User user)
        {
            try
            {
                Db.Users.Remove(user);
                await Db.SaveChangesAsync();
                successMessage = $"User '{user.FullName}' deleted successfully!";
                await LoadUsersAsync();
                await Task.Delay(2000);
                ClearMessages();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting user: {ex.Message}";
            }
        }

        private void ClearMessages()
        {
            errorMessage = string.Empty;
            successMessage = string.Empty;
        }

        private class UserFormModel
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "Student";
        }
    }
}
