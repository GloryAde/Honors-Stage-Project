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
        private List<Asset>? filteredAssets { get; set; }

        private bool showCreateForm = false;
        private bool isSubmitting = false;
        private string? errorMessage;
        private string? successMessage;
        private string equipmentSearchTerm = string.Empty;

        private LoanFormModel newLoan = new();
        private bool isAdmin = false;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                isAdmin = user.IsInRole("Admin");
            }

            await LoadLoansAsync();
            await LoadAvailableAssetsAsync();
        }

        private async Task LoadLoansAsync()
        {
            // All authenticated users can see all loans
            Loans = await Db.Loans
                .Include(l => l.Asset)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();
        }

        private async Task LoadAvailableAssetsAsync()
        {
            availableAssets = await Db.Assets
                .Where(a => a.IsAvailable)
                .OrderBy(a => a.Name)
                .ToListAsync();

            FilterAssets();
        }

        private void FilterAssets()
        {
            if (availableAssets == null)
            {
                filteredAssets = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(equipmentSearchTerm))
            {
                filteredAssets = availableAssets;
                return;
            }

            var searchTerm = equipmentSearchTerm.ToLower().Trim();
            filteredAssets = availableAssets
                .Where(a =>
                    a.ItemId.ToString().Contains(searchTerm) ||
                    a.PhysicalId.ToLower().Contains(searchTerm) ||
                    a.Name.ToLower().Contains(searchTerm) ||
                    a.Category.ToLower().Contains(searchTerm))
                .ToList();
        }

        private string EquipmentSearchTerm
        {
            get => equipmentSearchTerm;
            set
            {
                if (equipmentSearchTerm != value)
                {
                    equipmentSearchTerm = value;
                    FilterAssets();
                    StateHasChanged();
                }
            }
        }

        private void ShowCreateLoanForm()
        {
            showCreateForm = true;
            errorMessage = null;
            successMessage = null;
            equipmentSearchTerm = string.Empty;
            FilterAssets();
            newLoan = new LoanFormModel
            {
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7)
            };
        }

        private void CancelCreateLoan()
        {
            showCreateForm = false;
            errorMessage = null;
            successMessage = null;
            equipmentSearchTerm = string.Empty;
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

                if (string.IsNullOrWhiteSpace(newLoan.StudentFullName))
                {
                    errorMessage = "Please enter the student's full name.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(newLoan.StudentId))
                {
                    errorMessage = "Please enter the student's ID.";
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
                    StudentFullName = newLoan.StudentFullName.Trim(),
                    StudentId = newLoan.StudentId.Trim(),
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
            public string StudentFullName { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public DateTime LoanDate { get; set; } = DateTime.Today;
            public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        }
    }
}
