using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Text;

namespace HSP.Components.Pages
{
    public partial class Inventory : ComponentBase
    {
        [Inject]
        public IDbContextFactory<HspDbContext> DbFactory { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        private List<Asset>? assets;
        private List<Asset> filteredAssets = new();
        private Asset newAsset = new Asset();
        private bool showAddForm = false;
        private string message = string.Empty;
        private string errorMessage = string.Empty;
        private string searchTerm = string.Empty;
        private bool isCardView = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadAssetsAsync();
        }

        private async Task LoadAssetsAsync()
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            assets = await db.Assets.OrderBy(a => a.ItemId).ToListAsync();
            FilterAssets();
        }

        private void FilterAssets()
        {
            if (assets == null)
            {
                filteredAssets = new List<Asset>();
                return;
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredAssets = assets.ToList();
                return;
            }

            var search = searchTerm.ToLower().Trim();
            filteredAssets = assets
                .Where(a =>
                    a.ItemId.ToString().Contains(search) ||
                    a.PhysicalId.ToLower().Contains(search) ||
                    a.Name.ToLower().Contains(search) ||
                    a.Category.ToLower().Contains(search))
                .ToList();
        }

        private string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (searchTerm != value)
                {
                    searchTerm = value;
                    FilterAssets();
                    StateHasChanged();
                }
            }
        }

        private void ClearSearch()
        {
            searchTerm = string.Empty;
            FilterAssets();
        }

        private void ShowAddAssetForm()
        {
            showAddForm = true;
            newAsset = new Asset { IsAvailable = true };
            ClearMessages();
        }

        private void CancelAddAsset()
        {
            showAddForm = false;
            newAsset = new Asset();
            ClearMessages();
        }

        private async Task HandleAddAssetAsync()
        {
            try
            {
                if (!ValidateAsset(newAsset))
                {
                    errorMessage = "Please fill in all required fields.";
                    return;
                }

                await using var db = await DbFactory.CreateDbContextAsync();
                db.Assets.Add(newAsset);
                await db.SaveChangesAsync();

                message = $"Asset '{newAsset.Name}' added successfully!";
                errorMessage = string.Empty;

                await LoadAssetsAsync();

                await Task.Delay(2000);
                showAddForm = false;
                newAsset = new Asset();
                message = string.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error adding asset: {ex.Message}";
                message = string.Empty;
            }
        }

        private async Task DeleteAssetAsync(Asset asset)
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>(
                    "confirm",
                    $"Are you sure you want to delete '{asset.Name}'?");

                if (!confirmed)
                    return;

                await using var db = await DbFactory.CreateDbContextAsync();
                db.Assets.Remove(asset);
                await db.SaveChangesAsync();

                message = $"Asset '{asset.Name}' deleted successfully!";
                errorMessage = string.Empty;

                await LoadAssetsAsync();

                await Task.Delay(2000);
                message = string.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting asset: {ex.Message}";
                message = string.Empty;
            }
        }

        private async Task DownloadAssetListAsync()
        {
            try
            {
                if (assets == null || !assets.Any())
                {
                    errorMessage = "No assets to download.";
                    return;
                }

                var csv = new StringBuilder();
                csv.AppendLine("ID,Physical ID,Name,Category,Status");

                foreach (var asset in assets)
                {
                    var status = asset.IsAvailable ? "Available" : "On Loan";
                    csv.AppendLine($"{asset.ItemId},\"{asset.PhysicalId}\",\"{asset.Name}\",\"{asset.Category}\",{status}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var base64 = Convert.ToBase64String(bytes);
                var fileName = $"AssetList_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                await JSRuntime.InvokeVoidAsync("downloadFile", fileName, base64);

                message = "Asset list downloaded successfully!";
                await Task.Delay(2000);
                message = string.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error downloading asset list: {ex.Message}";
                message = string.Empty;
            }
        }

        private bool ValidateAsset(Asset asset)
        {
            return !string.IsNullOrWhiteSpace(asset.PhysicalId) &&
                   !string.IsNullOrWhiteSpace(asset.Name) && 
                   !string.IsNullOrWhiteSpace(asset.Category);
        }

        private void ClearMessages()
        {
            message = string.Empty;
            errorMessage = string.Empty;
        }

        private List<string> GetCategories()
        {
            if (assets == null || !assets.Any())
                return new List<string>();

            return assets.Select(a => a.Category).Distinct().OrderBy(c => c).ToList();
        }

        private List<string> GetFilteredCategories()
        {
            if (filteredAssets == null || !filteredAssets.Any())
                return new List<string>();

            return filteredAssets.Select(a => a.Category).Distinct().OrderBy(c => c).ToList();
        }

        private Microsoft.AspNetCore.Components.MarkupString GetCategoryIcon(string category)
        {
            var iconClass = category.ToLower() switch
            {
                var c when c.Contains("laptop") || c.Contains("computer") => "bi-laptop",
                var c when c.Contains("camera") || c.Contains("video") => "bi-camera-video",
                var c when c.Contains("audio") || c.Contains("headphone") || c.Contains("microphone") => "bi-headphones",
                var c when c.Contains("tablet") || c.Contains("ipad") => "bi-tablet",
                var c when c.Contains("projector") || c.Contains("presentation") => "bi-projector",
                var c when c.Contains("book") => "bi-book",
                var c when c.Contains("electronic") => "bi-cpu",
                var c when c.Contains("cable") || c.Contains("adapter") => "bi-plug",
                _ => "bi-box-seam"
            };

            return new Microsoft.AspNetCore.Components.MarkupString($"<i class='bi {iconClass}'></i>");
        }

        private Microsoft.AspNetCore.Components.MarkupString GetEquipmentIcon(string category)
        {
            return GetCategoryIcon(category);
        }
    }
}
