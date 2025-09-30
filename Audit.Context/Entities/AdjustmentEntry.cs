using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditPilot.Data.Entities
{
    public class AdjustmentEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FiscalPeriodId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public FiscalPeriod FiscalPeriod { get; set; } = default!;
        public ICollection<AdjustmentLine> Lines { get; set; } = new List<AdjustmentLine>();
    }
}
