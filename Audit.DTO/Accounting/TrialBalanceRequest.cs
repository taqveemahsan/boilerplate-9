namespace AuthPilot.Models.Accounting
{
    public class TrialBalanceRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IncludeUnposted { get; set; } = false;
        public bool IncludeZeroBalance { get; set; } = false;
    }
}

