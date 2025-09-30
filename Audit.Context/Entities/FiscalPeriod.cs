using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditPilot.Data.Entities
{
    public class FiscalPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public bool IsLocked { get; set; }

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; } = default!;
    }
}
