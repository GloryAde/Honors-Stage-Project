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
        public HspDbContext Db { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        private List<Asset>? assets;
        private Asset newAsset = new Asset();
        private bool showAddForm = false;
        private string message = string.Empty;
        private string errorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAssetsAsync();
        }

        private async Task LoadAssetsAsync()
        {
            assets = await Db.Assets.OrderBy(a => a.ItemId).ToListAsync();
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

                Db.Assets.Add(newAsset);
                await Db.SaveChangesAsync();

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

                Db.Assets.Remove(asset);
                await Db.SaveChangesAsync();

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
                csv.AppendLine("ID,Name,Category,Status");

                foreach (var asset in assets)
                {
                    var status = asset.IsAvailable ? "Available" : "On Loan";
                    csv.AppendLine($"{asset.ItemId},\"{asset.Name}\",\"{asset.Category}\",{status}");
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
            return !string.IsNullOrWhiteSpace(asset.Name) && 
                   !string.IsNullOrWhiteSpace(asset.Category);
        }

        private void ClearMessages()
        {
            message = string.Empty;
            errorMessage = string.Empty;
        }
    }
}
