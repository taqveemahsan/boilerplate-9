using AuthPilot.Models.Adjustments;
using AuthPilot.Models.FiscalPeriods;

namespace Audit.Services.Interfaces
{
    public interface IAdjustmentService
    {
        Task<PagedResult<AdjustmentEntryDto>> ListAsync(AdjustmentEntryQuery query, CancellationToken ct);
        Task<AdjustmentEntryDto?> GetAsync(int id, CancellationToken ct);
        Task<AdjustmentEntryDto> CreateAsync(AdjustmentEntryCreateRequest request, CancellationToken ct);
        Task<AdjustmentEntryDto> UpdateAsync(int id, AdjustmentEntryUpdateRequest request, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}
