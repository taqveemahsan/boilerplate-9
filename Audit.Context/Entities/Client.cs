using Audit.Common;
using System.ComponentModel.DataAnnotations;

namespace AuditPilot.Data.Entities
{
    public class Client
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public CompanyType CompanyType { get; set; }

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

