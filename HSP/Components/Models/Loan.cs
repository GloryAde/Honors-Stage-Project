using System;

namespace HSP.Models
{
    public class Loan
    {
        public int LoanId { get; set; } // EF will auto-recognize this as PK

        // Foreign key to Asset
        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
