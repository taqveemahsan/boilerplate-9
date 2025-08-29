using System.ComponentModel.DataAnnotations;
using Audit.Common;

namespace AuthPilot.Models.Clients
{
    public class UpdateClientModel
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public CompanyType CompanyType { get; set; }

        [MaxLength(200)]
        public string? CompanyName { get; set; }
    }
}

