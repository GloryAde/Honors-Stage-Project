using HSP.Data;
using HSP.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace HSP.Components.Pages
{
    public partial class Loan : ComponentBase
    {
        [Inject]
        public HspDbContext Db { get; set; } = default!;

        private List<HSP.Models.Loan>? Loans { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadLoansAsync();
        }

        private async Task LoadLoansAsync()
        {
            Loans = await Db.Loans
                .Include(l => l.Asset)
                .Include(l => l.User)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();
        }
    }
}
