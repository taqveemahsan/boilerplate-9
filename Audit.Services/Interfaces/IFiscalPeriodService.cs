using AuthPilot.Models.FiscalPeriods;

namespace Audit.Services.Interfaces
{
    public interface IFiscalPeriodService
    {
        Task<PagedResult<FiscalPeriodDto>> ListAsync(FiscalPeriodQuery query, CancellationToken ct);
        Task<FiscalPeriodDto?> GetAsync(int id, CancellationToken ct);
        Task<FiscalPeriodDto> CreateAsync(FiscalPeriodCreateRequest request, CancellationToken ct);
        Task<FiscalPeriodDto> UpdateAsync(int id, FiscalPeriodUpdateRequest request, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
        Task<FiscalPeriodDto> LockAsync(int id, CancellationToken ct);
    }
}
