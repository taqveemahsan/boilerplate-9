using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditPilot.Data.Entities
{
    public class AdjustmentLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdjustmentEntryId { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        [MaxLength(500)]
        public string? LineMemo { get; set; }

        public AdjustmentEntry AdjustmentEntry { get; set; } = default!;
        public Account Account { get; set; } = default!;
    }
}
