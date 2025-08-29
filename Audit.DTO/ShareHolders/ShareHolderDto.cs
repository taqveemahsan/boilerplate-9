namespace AuthPilot.Models.ShareHolders
{
    public class ShareHolderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Cnic { get; set; }
        public string? CompanyName { get; set; }
        public decimal Percentage { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
