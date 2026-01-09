using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace HSP.Components.Pages
{
    public partial class Inventory : ComponentBase
    {
        [Inject]
        public HspDbContext Db { get; set; } = default!;

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
