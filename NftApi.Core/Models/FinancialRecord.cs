using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NftApi.Core.Models
{
    public class FinancialRecord
    {
        [Key]
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string AccountType { get; set; } = string.Empty;

        public decimal InitialAmount { get; set; }
        public decimal? TotalPaymentAmount { get; set; }
        public decimal? RepaymentAmount { get; set; }
        public decimal? RemainingAmount { get; set; }

        public DateTime TransactionDate { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

        public decimal? MinimumPaymentAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public int? InitialTerm { get; set; }
        public int? RemainingTerm { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public bool IsOpen => Status != "Closed" && RemainingAmount > 0;
    }
}
