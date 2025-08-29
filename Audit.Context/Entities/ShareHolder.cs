using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditPilot.Data.Entities
{
    public class ShareHolder
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();


        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(25)]
        public string? Cnic { get; set; }

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [Required]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
