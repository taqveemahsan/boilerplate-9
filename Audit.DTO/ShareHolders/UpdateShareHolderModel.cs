using System.ComponentModel.DataAnnotations;

namespace AuthPilot.Models.ShareHolders
{
    public class UpdateShareHolderModel
    {
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
        public decimal Percentage { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
