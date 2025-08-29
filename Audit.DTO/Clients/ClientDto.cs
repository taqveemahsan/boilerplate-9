using Audit.Common;

namespace AuthPilot.Models.Clients
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CompanyType CompanyType { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

