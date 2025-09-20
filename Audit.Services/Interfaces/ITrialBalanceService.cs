using AuthPilot.Models.Accounting;

namespace Audit.Services.Interfaces
{
    public interface ITrialBalanceService
    {
        Task<TrialBalanceDto> GetAsync(TrialBalanceRequest request, CancellationToken ct = default);

        // Trial Balance Rows (persisted) APIs
        Task<AuthPilot.Models.TrialBalanceRows.PagedResult<AuthPilot.Models.TrialBalanceRows.TrialBalanceRowDto>> ListAsync(AuthPilot.Models.TrialBalanceRows.TrialBalanceQuery q, CancellationToken ct);
        Task<AuthPilot.Models.TrialBalanceRows.TrialBalanceRowDto?> GetAsync(int id, CancellationToken ct);
        Task<AuthPilot.Models.TrialBalanceRows.TrialBalanceRowDto> UpdateAsync(int id, AuthPilot.Models.TrialBalanceRows.TrialBalanceRowUpdateDto dto, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
        Task<AuthPilot.Models.TrialBalanceRows.TrialBalanceImportResponse> ImportAsync(int fiscalPeriodId, System.IO.Stream fileStream, string fileName, bool force, CancellationToken ct);
        Task<(byte[] Content, string FileName, string ContentType)> ExportAsync(int fiscalPeriodId, string format, CancellationToken ct);
        Task<AuthPilot.Models.TrialBalanceRows.TrialBalanceSummaryDto> SummaryAsync(int fiscalPeriodId, CancellationToken ct);
    }
}
