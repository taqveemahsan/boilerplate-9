namespace AuthPilot.Models.Accounting
{
    public class TrialBalanceDto
    {
        public IReadOnlyList<TrialBalanceRowDto> Rows { get; set; } = Array.Empty<TrialBalanceRowDto>();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}

