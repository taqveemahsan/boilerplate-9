using AuthPilot.Models.Clients;

namespace Audit.Services.Interfaces
{
    public interface IClientService
    {
        Task<ClientDto?> CreateAsync(CreateClientModel model, CancellationToken ct = default);
        Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid id, UpdateClientModel model, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}

