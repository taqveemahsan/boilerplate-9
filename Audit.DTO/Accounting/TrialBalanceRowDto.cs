namespace AuthPilot.Models.Accounting
{
    public class TrialBalanceRowDto
    {
        public Guid AccountId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}

