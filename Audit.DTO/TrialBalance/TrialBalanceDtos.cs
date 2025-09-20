namespace AuthPilot.Models.TrialBalanceRows
{
    public record TrialBalanceQuery
    {
        public int FiscalPeriodId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 50;
        public string? Search { get; init; }
        public string? AccountCode { get; init; }
        public string? SortBy { get; init; } = "Code";
        public string? SortDir { get; init; } = "asc";
    }

    public record TrialBalanceRowDto
    {
        public int Id { get; init; }
        public int FiscalPeriodId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string? Level1 { get; init; }
        public string? Level2 { get; init; }
        public string? Level3 { get; init; }
        public string? Level4 { get; init; }
        public decimal CY_Debit { get; init; }
        public decimal CY_Credit { get; init; }
        public decimal Adj_Debit { get; init; }
        public decimal Adj_Credit { get; init; }
        public decimal CY_Adjusted_Debit { get; init; }
        public decimal CY_Adjusted_Credit { get; init; }
        public decimal PY_Debit { get; init; }
        public decimal PY_Credit { get; init; }
        public string? Notes { get; init; }
        public byte[]? RowVersion { get; init; }
    }

    public record TrialBalanceRowUpdateDto
    {
        public decimal CY_Debit { get; init; }
        public decimal CY_Credit { get; init; }
        public decimal Adj_Debit { get; init; }
        public decimal Adj_Credit { get; init; }
        public decimal PY_Debit { get; init; }
        public decimal PY_Credit { get; init; }
        public string? Notes { get; init; }
        public byte[] RowVersion { get; init; } = default!;
    }

    public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);

    public record TrialBalanceSummaryDto(
        decimal TotalCY_Debit, decimal TotalCY_Credit,
        decimal TotalAdj_Debit, decimal TotalAdj_Credit,
        decimal TotalAdjCY_Debit, decimal TotalAdjCY_Credit,
        decimal TotalPY_Debit, decimal TotalPY_Credit,
        bool IsBalanced);

    public record TrialBalanceImportResponse(Guid JobId, string Status, int Inserted, int Updated, IEnumerable<string> Warnings, IEnumerable<string> Errors);
}

