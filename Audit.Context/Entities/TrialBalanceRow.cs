using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditPilot.Data.Entities
{
    public class TrialBalanceRow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FiscalPeriodId { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CY_Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CY_Credit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Adj_Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Adj_Credit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CY_Adjusted_Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CY_Adjusted_Credit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PY_Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PY_Credit { get; set; }

        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;

        public Account Account { get; set; } = default!;
        public FiscalPeriod Period { get; set; } = default!;
    }
}

