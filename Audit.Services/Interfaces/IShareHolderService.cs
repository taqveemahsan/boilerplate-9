using AuthPilot.Models.ShareHolders;

namespace Audit.Services.Interfaces
{
    public interface IShareHolderService
    {
        Task<IReadOnlyList<ShareHolderDto>> GetAllAsync(CancellationToken ct = default);
        Task<ShareHolderDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ShareHolderDto?> CreateAsync(CreateShareHolderModel model, CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid id, UpdateShareHolderModel model, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
