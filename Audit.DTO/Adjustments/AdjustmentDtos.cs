using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthPilot.Models.Adjustments
{
    public record AdjustmentEntryLineRequest
    {
        [Required]
        [MaxLength(50)]
        public string AccountCode { get; init; } = string.Empty;

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Debit must be non-negative")]
        public decimal Debit { get; init; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Credit must be non-negative")]
        public decimal Credit { get; init; }

        [MaxLength(500)]
        public string? LineMemo { get; init; }
    }

    public record AdjustmentEntryCreateRequest
    {
        [Required]
        public int FiscalPeriodId { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one line is required")]
        public IReadOnlyList<AdjustmentEntryLineRequest> Lines { get; init; } = Array.Empty<AdjustmentEntryLineRequest>();
    }

    public record AdjustmentEntryUpdateRequest
    {
        [Required]
        public int FiscalPeriodId { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one line is required")]
        public IReadOnlyList<AdjustmentEntryLineRequest> Lines { get; init; } = Array.Empty<AdjustmentEntryLineRequest>();
    }

    public record AdjustmentEntryQuery
    {
        [Required]
        public int FiscalPeriodId { get; init; }

        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;

        [Range(1, 500)]
        public int PageSize { get; init; } = 50;
    }

    public record AdjustmentLineDto
    {
        public int Id { get; init; }
        public Guid AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public decimal Debit { get; init; }
        public decimal Credit { get; init; }
        public string? LineMemo { get; init; }
    }

    public record AdjustmentEntryDto
    {
        public int Id { get; init; }
        public int FiscalPeriodId { get; init; }
        public string? Description { get; init; }
        public DateTime PostedAtUtc { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
        public IReadOnlyList<AdjustmentLineDto> Lines { get; init; } = Array.Empty<AdjustmentLineDto>();
    }
}
