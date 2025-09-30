using System.ComponentModel.DataAnnotations;

namespace AuthPilot.Models.FiscalPeriods
{
    public record FiscalPeriodDto
    {
        public int Id { get; init; }
        public Guid ClientId { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public bool IsLocked { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }

    public record FiscalPeriodCreateRequest
    {
        [Required]
        public Guid ClientId { get; init; }

        [Required]
        [MaxLength(200)]
        public string Name { get; init; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; init; }

        [Required]
        public DateOnly EndDate { get; init; }
    }

    public record FiscalPeriodUpdateRequest
    {
        [Required]
        public Guid ClientId { get; init; }

        [Required]
        [MaxLength(200)]
        public string Name { get; init; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; init; }

        [Required]
        public DateOnly EndDate { get; init; }
    }

    public record FiscalPeriodQuery
    {
        public Guid? ClientId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 50;
        public string? Search { get; init; }
    }

    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
}
