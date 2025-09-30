using Audit.Services.Interfaces;
using AuditPilot.Data;
using AuditPilot.Data.Entities;
using AuthPilot.Models.Adjustments;
using AuthPilot.Models.FiscalPeriods;
using Microsoft.EntityFrameworkCore;

namespace Audit.Services
{
    public class AdjustmentService : IAdjustmentService
    {
        private readonly ApplicationDbContext _db;

        public AdjustmentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<AdjustmentEntryDto>> ListAsync(AdjustmentEntryQuery query, CancellationToken ct)
        {
            if (query.FiscalPeriodId <= 0)
            {
                throw new ArgumentException("fiscalPeriodId is required");
            }

            if (query.Page <= 0)
            {
                throw new ArgumentException("page must be >= 1");
            }

            if (query.PageSize <= 0 || query.PageSize > 500)
            {
                throw new ArgumentException("pageSize must be between 1 and 500");
            }

            var baseQuery = _db.AdjustmentEntries
                .AsNoTracking()
                .Where(e => e.FiscalPeriodId == query.FiscalPeriodId)
                .OrderByDescending(e => e.PostedAtUtc)
                .ThenByDescending(e => e.Id);

            var total = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new AdjustmentEntryDto
                {
                    Id = e.Id,
                    FiscalPeriodId = e.FiscalPeriodId,
                    Description = e.Description,
                    PostedAtUtc = e.PostedAtUtc,
                    CreatedAtUtc = e.CreatedAtUtc,
                    UpdatedAtUtc = e.UpdatedAtUtc,
                    Lines = e.Lines
                        .OrderBy(l => l.Id)
                        .Select(l => new AdjustmentLineDto
                        {
                            Id = l.Id,
                            AccountId = l.AccountId,
                            AccountCode = l.Account.Code,
                            AccountName = l.Account.Name,
                            Debit = l.Debit,
                            Credit = l.Credit,
                            LineMemo = l.LineMemo
                        })
                        .ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<AdjustmentEntryDto>(items, total, query.Page, query.PageSize);
        }

        public async Task<AdjustmentEntryDto?> GetAsync(int id, CancellationToken ct)
        {
            return await _db.AdjustmentEntries
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new AdjustmentEntryDto
                {
                    Id = e.Id,
                    FiscalPeriodId = e.FiscalPeriodId,
                    Description = e.Description,
                    PostedAtUtc = e.PostedAtUtc,
                    CreatedAtUtc = e.CreatedAtUtc,
                    UpdatedAtUtc = e.UpdatedAtUtc,
                    Lines = e.Lines
                        .OrderBy(l => l.Id)
                        .Select(l => new AdjustmentLineDto
                        {
                            Id = l.Id,
                            AccountId = l.AccountId,
                            AccountCode = l.Account.Code,
                            AccountName = l.Account.Name,
                            Debit = l.Debit,
                            Credit = l.Credit,
                            LineMemo = l.LineMemo
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<AdjustmentEntryDto> CreateAsync(AdjustmentEntryCreateRequest request, CancellationToken ct)
        {
            ValidateLines(request.Lines, out var normalizedLines, out var errors);
            if (errors.Count > 0)
            {
                throw new AdjustmentValidationException(errors);
            }

            var period = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.FiscalPeriodId, ct);
            if (period == null)
            {
                throw new ArgumentException("Fiscal period not found");
            }

            if (period.IsLocked)
            {
                throw new InvalidOperationException("Fiscal period is locked");
            }

            var accountCodes = normalizedLines.Select(l => l.AccountCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var accounts = await _db.Accounts
                .Where(a => accountCodes.Contains(a.Code))
                .ToListAsync(ct);

            if (accounts.Count != accountCodes.Count)
            {
                var missing = accountCodes
                    .Where(code => accounts.All(a => !string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                throw new ArgumentException($"Unknown account codes: {string.Join(", ", missing)}");
            }

            var accountLookup = accounts.ToDictionary(a => a.Code, StringComparer.OrdinalIgnoreCase);
            var utcNow = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var entry = new AdjustmentEntry
            {
                FiscalPeriodId = request.FiscalPeriodId,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                PostedAtUtc = utcNow,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            };

            var accountIds = normalizedLines.Select(l => accountLookup[l.AccountCode].Id).Distinct().ToList();
            var existingRows = await LoadTrialBalanceRowsAsync(request.FiscalPeriodId, accountIds, ct);

            foreach (var line in normalizedLines)
            {
                var account = accountLookup[line.AccountCode];
                var newLine = new AdjustmentLine
                {
                    AccountId = account.Id,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    LineMemo = line.LineMemo
                };
                entry.Lines.Add(newLine);

                var row = GetOrCreateTrialBalanceRow(existingRows, request.FiscalPeriodId, account.Id);
                ApplyTrialBalanceDelta(row, line.Debit, line.Credit, utcNow);
            }

            _db.AdjustmentEntries.Add(entry);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return (await GetAsync(entry.Id, ct))!;
        }

        public async Task<AdjustmentEntryDto> UpdateAsync(int id, AdjustmentEntryUpdateRequest request, CancellationToken ct)
        {
            ValidateLines(request.Lines, out var normalizedLines, out var errors);
            if (errors.Count > 0)
            {
                throw new AdjustmentValidationException(errors);
            }

            var entry = await _db.AdjustmentEntries
                .Include(e => e.Lines)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            if (entry == null)
            {
                throw new KeyNotFoundException();
            }

            var originalPeriodId = entry.FiscalPeriodId;
            var originalPeriod = await _db.FiscalPeriods.FirstAsync(p => p.Id == originalPeriodId, ct);
            if (originalPeriod.IsLocked)
            {
                throw new InvalidOperationException("Fiscal period is locked");
            }

            if (request.FiscalPeriodId <= 0)
            {
                throw new ArgumentException("Fiscal period is required");
            }

            var targetPeriod = originalPeriod;
            if (request.FiscalPeriodId != originalPeriodId)
            {
                targetPeriod = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.FiscalPeriodId, ct)
                    ?? throw new ArgumentException("Target fiscal period not found");

                if (targetPeriod.IsLocked)
                {
                    throw new InvalidOperationException("Fiscal period is locked");
                }
            }

            var accountCodes = normalizedLines.Select(l => l.AccountCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var accounts = await _db.Accounts
                .Where(a => accountCodes.Contains(a.Code))
                .ToListAsync(ct);

            if (accounts.Count != accountCodes.Count)
            {
                var missing = accountCodes
                    .Where(code => accounts.All(a => !string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                throw new ArgumentException($"Unknown account codes: {string.Join(", ", missing)}");
            }

            var accountLookup = accounts.ToDictionary(a => a.Code, StringComparer.OrdinalIgnoreCase);
            var utcNow = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var oldAccountIds = entry.Lines.Select(l => l.AccountId).Distinct().ToList();
            var oldRows = await LoadTrialBalanceRowsAsync(originalPeriodId, oldAccountIds, ct);

            foreach (var line in entry.Lines)
            {
                var row = GetOrCreateTrialBalanceRow(oldRows, originalPeriodId, line.AccountId);
                ApplyTrialBalanceDelta(row, -line.Debit, -line.Credit, utcNow);
            }

            _db.AdjustmentLines.RemoveRange(entry.Lines);

            entry.FiscalPeriodId = request.FiscalPeriodId;
            entry.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entry.PostedAtUtc = utcNow;
            entry.UpdatedAtUtc = utcNow;
            entry.Lines = new List<AdjustmentLine>();

            var newAccountIds = normalizedLines.Select(l => accountLookup[l.AccountCode].Id).Distinct().ToList();
            Dictionary<Guid, TrialBalanceRow> newRows;
            if (request.FiscalPeriodId == originalPeriodId)
            {
                newRows = oldRows;
            }
            else
            {
                newRows = await LoadTrialBalanceRowsAsync(request.FiscalPeriodId, newAccountIds, ct);
            }

            foreach (var line in normalizedLines)
            {
                var account = accountLookup[line.AccountCode];
                var newLine = new AdjustmentLine
                {
                    AccountId = account.Id,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    LineMemo = line.LineMemo
                };
                entry.Lines.Add(newLine);

                var row = GetOrCreateTrialBalanceRow(newRows, request.FiscalPeriodId, account.Id);
                ApplyTrialBalanceDelta(row, line.Debit, line.Credit, utcNow);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return (await GetAsync(entry.Id, ct))!;
        }
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entry = await _db.AdjustmentEntries
                .Include(e => e.Lines)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            if (entry == null)
            {
                throw new KeyNotFoundException();
            }

            var period = await _db.FiscalPeriods.FirstAsync(p => p.Id == entry.FiscalPeriodId, ct);
            if (period.IsLocked)
            {
                throw new InvalidOperationException("Fiscal period is locked");
            }

            var utcNow = DateTime.UtcNow;
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var accountIds = entry.Lines.Select(l => l.AccountId).Distinct().ToList();
            var rows = await LoadTrialBalanceRowsAsync(entry.FiscalPeriodId, accountIds, ct);
            foreach (var line in entry.Lines)
            {
                var row = GetOrCreateTrialBalanceRow(rows, entry.FiscalPeriodId, line.AccountId);
                ApplyTrialBalanceDelta(row, -line.Debit, -line.Credit, utcNow);
            }

            _db.AdjustmentEntries.Remove(entry);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        private static void ValidateLines(IReadOnlyList<AdjustmentEntryLineRequest> lines, out List<NormalizedLine> normalized, out List<string> errors)
        {
            normalized = new List<NormalizedLine>(lines.Count);
            errors = new List<string>();

            decimal totalDebit = 0m;
            decimal totalCredit = 0m;

            for (var i = 0; i < lines.Count; i++)
            {
                var source = lines[i];
                var label = $"Line {i + 1}";
                var code = (source.AccountCode ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(code))
                {
                    errors.Add($"{label}: accountCode is required");
                    continue;
                }

                if (source.Debit < 0)
                {
                    errors.Add($"{label}: debit must be >= 0");
                }

                if (source.Credit < 0)
                {
                    errors.Add($"{label}: credit must be >= 0");
                }

                if (source.Debit > 0 && source.Credit > 0)
                {
                    errors.Add($"{label}: specify either debit or credit, not both");
                }

                if (source.Debit == 0 && source.Credit == 0)
                {
                    errors.Add($"{label}: debit or credit must be provided");
                }

                totalDebit += source.Debit;
                totalCredit += source.Credit;

                normalized.Add(new NormalizedLine(code, Math.Round(source.Debit, 2), Math.Round(source.Credit, 2), string.IsNullOrWhiteSpace(source.LineMemo) ? null : source.LineMemo.Trim()));
            }

            if (Math.Round(totalDebit, 2) != Math.Round(totalCredit, 2))
            {
                errors.Add("Lines must balance (total debit equals total credit)");
            }
        }

        private async Task<Dictionary<Guid, TrialBalanceRow>> LoadTrialBalanceRowsAsync(int fiscalPeriodId, IReadOnlyCollection<Guid> accountIds, CancellationToken ct)
        {
            var rows = new Dictionary<Guid, TrialBalanceRow>();
            if (accountIds.Count == 0)
            {
                return rows;
            }

            var entities = await _db.TrialBalanceRows
                .Where(r => r.FiscalPeriodId == fiscalPeriodId && accountIds.Contains(r.AccountId))
                .ToListAsync(ct);

            foreach (var row in entities)
            {
                rows[row.AccountId] = row;
            }

            return rows;
        }

        private TrialBalanceRow GetOrCreateTrialBalanceRow(Dictionary<Guid, TrialBalanceRow> cache, int fiscalPeriodId, Guid accountId)
        {
            if (cache.TryGetValue(accountId, out var existing))
            {
                return existing;
            }

            var row = new TrialBalanceRow
            {
                FiscalPeriodId = fiscalPeriodId,
                AccountId = accountId,
                CY_Debit = 0,
                CY_Credit = 0,
                Adj_Debit = 0,
                Adj_Credit = 0,
                CY_Adjusted_Debit = 0,
                CY_Adjusted_Credit = 0,
                PY_Debit = 0,
                PY_Credit = 0,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.TrialBalanceRows.Add(row);
            cache[accountId] = row;
            return row;
        }

        private static void ApplyTrialBalanceDelta(TrialBalanceRow row, decimal debitDelta, decimal creditDelta, DateTime utcNow)
        {
            row.Adj_Debit += debitDelta;
            row.Adj_Credit += creditDelta;
            row.CY_Adjusted_Debit = row.CY_Debit + row.Adj_Debit;
            row.CY_Adjusted_Credit = row.CY_Credit + row.Adj_Credit;
            row.UpdatedAtUtc = utcNow;
        }

        private sealed record NormalizedLine(string AccountCode, decimal Debit, decimal Credit, string? LineMemo);

    }
}


