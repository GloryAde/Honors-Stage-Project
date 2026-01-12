using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace HSP.Components.Pages
{
    public partial class Loan : ComponentBase
    {
        [Inject]
        public HspDbContext Db { get; set; } = default!;

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private List<HSP.Models.Loan>? Loans { get; set; }
        private List<Asset>? availableAssets { get; set; }
        private List<User>? users { get; set; }
        
        private bool showCreateForm = false;
        private bool isSubmitting = false;
        private string? errorMessage;
        private string? successMessage;
        
        private LoanFormModel newLoan = new();
        private int currentUserId = 0;
        private bool isAdmin = false;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                currentUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                isAdmin = user.IsInRole("Admin");
            }

            await LoadLoansAsync();
            await LoadAvailableAssetsAsync();
            
            if (isAdmin)
            {
                await LoadUsersAsync();
            }
        }

        private async Task LoadLoansAsync()
        {
            if (isAdmin)
            {
                // Admin sees all loans
                Loans = await Db.Loans
                    .Include(l => l.Asset)
                    .Include(l => l.User)
                    .OrderByDescending(l => l.LoanDate)
                    .ToListAsync();
            }
            else
            {
                // Students see only their loans
                Loans = await Db.Loans
                    .Include(l => l.Asset)
                    .Include(l => l.User)
                    .Where(l => l.UserId == currentUserId)
                    .OrderByDescending(l => l.LoanDate)
                    .ToListAsync();
            }
        }

        private async Task LoadAvailableAssetsAsync()
        {
            availableAssets = await Db.Assets
                .Where(a => a.IsAvailable)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        private async Task LoadUsersAsync()
        {
            users = await Db.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        private void ShowCreateLoanForm()
        {
            showCreateForm = true;
            errorMessage = null;
            successMessage = null;
            newLoan = new LoanFormModel
            {
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7),
                UserId = isAdmin ? 0 : currentUserId // Students loan to themselves
            };
        }

        private void CancelCreateLoan()
        {
            showCreateForm = false;
            errorMessage = null;
            successMessage = null;
            newLoan = new();
        }

        private async Task CreateLoanAsync()
        {
            errorMessage = null;
            successMessage = null;
            isSubmitting = true;

            try
            {
                // Validate
                if (newLoan.AssetId == 0)
                {
                    errorMessage = "Please select an equipment item.";
                    return;
                }

                // For students, always use their own ID
                if (!isAdmin)
                {
                    newLoan.UserId = currentUserId;
                }

                if (newLoan.UserId == 0)
                {
                    errorMessage = "Please select a user.";
                    return;
                }

                if (newLoan.DueDate <= newLoan.LoanDate)
                {
                    errorMessage = "Due date must be after loan date.";
                    return;
                }

                // Check if asset is available
                var asset = await Db.Assets.FindAsync(newLoan.AssetId);
                if (asset == null)
                {
                    errorMessage = "Selected equipment not found.";
                    return;
                }

                if (!asset.IsAvailable)
                {
                    errorMessage = "Selected equipment is not available.";
                    return;
                }

                // Create loan
                var loan = new HSP.Models.Loan
                {
                    AssetId = newLoan.AssetId,
                    UserId = newLoan.UserId,
                    LoanDate = newLoan.LoanDate,
                    DueDate = newLoan.DueDate,
                    ReturnDate = null
                };

                Db.Loans.Add(loan);

                // Mark asset as unavailable
                asset.IsAvailable = false;

                await Db.SaveChangesAsync();

                successMessage = "Loan created successfully!";
                
                // Refresh data
                await LoadLoansAsync();
                await LoadAvailableAssetsAsync();

                // Reset form after a short delay to show success message
                await Task.Delay(1500);
                CancelCreateLoan();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error creating loan: {ex.Message}";
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }

        private async Task MarkAsReturnedAsync(HSP.Models.Loan loan)
        {
            if (!isAdmin)
            {
                errorMessage = "Only administrators can mark loans as returned.";
                return;
            }

            try
            {
                loan.ReturnDate = DateTime.Now;
                
                // Mark asset as available again
                if (loan.Asset != null)
                {
                    loan.Asset.IsAvailable = true;
                }
                else
                {
                    var asset = await Db.Assets.FindAsync(loan.AssetId);
                    if (asset != null)
                    {
                        asset.IsAvailable = true;
                    }
                }

                await Db.SaveChangesAsync();
                await LoadLoansAsync();
                await LoadAvailableAssetsAsync();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error marking loan as returned: {ex.Message}";
            }
        }

        private class LoanFormModel
        {
            public int AssetId { get; set; }
            public int UserId { get; set; }
            public DateTime LoanDate { get; set; } = DateTime.Today;
            public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        }
    }
}
