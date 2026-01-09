using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace HSP.Components.Pages
{
    public partial class Users : ComponentBase
    {
        [Inject]
        public HspDbContext Db { get; set; } = default!;

        private List<User>? UserList { get; set; }

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
    }
}
