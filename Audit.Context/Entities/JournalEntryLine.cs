using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditPilot.Data.Entities
{
    public class JournalEntryLine
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JournalEntryId { get; set; }
        public JournalEntry JournalEntry { get; set; } = null!;

        [Required]
        public Guid AccountId { get; set; }
        public Account Account { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        [MaxLength(300)]
        public string? Memo { get; set; }
    }
}

