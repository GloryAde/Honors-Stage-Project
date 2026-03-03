using System;
using System.ComponentModel.DataAnnotations;

namespace HSP.Models
{
    public class Loan
    {
        public int LoanId { get; set; } // EF will auto-recognize this as PK

        // Foreign key to Asset
        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        // Student Information (no longer using User reference)
        [Required]
        public string StudentFullName { get; set; } = string.Empty;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
