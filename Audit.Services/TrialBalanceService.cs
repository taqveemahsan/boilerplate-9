using Audit.Services.Interfaces;
using AuditPilot.Data;
using Microsoft.EntityFrameworkCore;
using AuthPilot.Models.Accounting;
using TB = AuthPilot.Models.TrialBalanceRows;
using AuditPilot.Data.Entities;
using System.Globalization;

namespace Audit.Services
{
    public class TrialBalanceService : ITrialBalanceService
    {
        private readonly ApplicationDbContext _db;

        public TrialBalanceService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<TrialBalanceDto> GetAsync(TrialBalanceRequest request, CancellationToken ct = default)
        {
            if (request.ToDate < request.FromDate)
                throw new ArgumentException("ToDate must be on or after FromDate");

            var fromDate = request.FromDate.Date;
            var toDate = request.ToDate.Date.AddDays(1).AddTicks(-1);

            // 1) Pre-aggregate with null-safe SUMs (decimal? -> coalesce to 0m)
            var linesAgg =
                from jl in _db.JournalEntryLines.AsNoTracking()
                join j in _db.JournalEntries.AsNoTracking() on jl.JournalEntryId equals j.Id
                where j.Date >= fromDate
                   && j.Date <= toDate
                   && (request.IncludeUnposted || j.Posted)
                group jl by jl.AccountId into g
                select new
                {
                    AccountId = g.Key,
                    Debit = g.Sum(x => (decimal?)x.Debit) ?? 0m,
                    Credit = g.Sum(x => (decimal?)x.Credit) ?? 0m
                };

            // 2) Left join Accounts to aggregates
            var query =
                from a in _db.Accounts.AsNoTracking()
                join l in linesAgg on a.Id equals l.AccountId into gj
                from agg in gj.DefaultIfEmpty()
                where request.IncludeZeroBalance
                   || (agg != null && (agg.Debit != 0m || agg.Credit != 0m))
                select new TrialBalanceRowDto
                {
                    AccountId = a.Id,              // PK non-null
                    Code = a.Code,            // if nullable & you want: a.Code ?? ""
                    Name = a.Name,
                    //Debit = agg.Debit, // ✅ null-safe projection
                    //Credit = agg.Credit  // ✅ null-safe projection
                };

            var rows = await query
                .OrderBy(r => r.Code ?? "")        // ✅ safe ordering if Code can be null
                .ToListAsync(ct);

            return new TrialBalanceDto
            {
                Rows = rows,
                TotalDebit = rows.Sum(r => r.Debit),
                TotalCredit = rows.Sum(r => r.Credit)
            };
        }

        
        // -------------------- Persisted Trial Balance Rows APIs --------------------

        public async Task<TB.PagedResult<TB.TrialBalanceRowDto>> ListAsync(TB.TrialBalanceQuery q, CancellationToken ct)
        {
            if (q.FiscalPeriodId <= 0) throw new ArgumentException("FiscalPeriodId is required");
            if (q.Page < 1) throw new ArgumentException("Page must be >= 1");
            if (q.PageSize < 1 || q.PageSize > 500) throw new ArgumentException("PageSize must be between 1 and 500");

            var query = _db.TrialBalanceRows
                .AsNoTracking()
                .Include(r => r.Account)
                .Where(r => r.FiscalPeriodId == q.FiscalPeriodId);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(r => r.Account != null && (r.Account.Code.Contains(s) || r.Account.Name.Contains(s)));
            }
            if (!string.IsNullOrWhiteSpace(q.AccountCode))
            {
                var ac = q.AccountCode.Trim();
                query = query.Where(r => r.Account != null && r.Account.Code == ac);
            }

            var total = await query.CountAsync(ct);

            bool asc = string.Equals(q.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (q.SortBy ?? "Code").ToLowerInvariant() switch
            {
                "name" => asc ? query.OrderBy(r => r.Account != null ? r.Account.Name : "") : query.OrderByDescending(r => r.Account != null ? r.Account.Name : ""),
                "cy_debit" => asc ? query.OrderBy(r => r.CY_Debit) : query.OrderByDescending(r => r.CY_Debit),
                "cy_credit" => asc ? query.OrderBy(r => r.CY_Credit) : query.OrderByDescending(r => r.CY_Credit),
                "cy_adjusted_debit" => asc ? query.OrderBy(r => r.CY_Adjusted_Debit) : query.OrderByDescending(r => r.CY_Adjusted_Debit),
                "cy_adjusted_credit" => asc ? query.OrderBy(r => r.CY_Adjusted_Credit) : query.OrderByDescending(r => r.CY_Adjusted_Credit),
                _ => asc ? query.OrderBy(r => r.Account != null ? r.Account.Code : "") : query.OrderByDescending(r => r.Account != null ? r.Account.Code : "")
            };

            var items = await query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(r => new TB.TrialBalanceRowDto
                {
                    Id = r.Id,
                    FiscalPeriodId = r.FiscalPeriodId,
                    AccountCode = r.Account != null ? r.Account.Code : "",
                    AccountName = r.Account != null ? r.Account.Name : "",
                    Level1 = null,
                    Level2 = null,
                    Level3 = null,
                    Level4 = null,
                    CY_Debit = r.CY_Debit,
                    CY_Credit = r.CY_Credit,
                    Adj_Debit = r.Adj_Debit,
                    Adj_Credit = r.Adj_Credit,
                    CY_Adjusted_Debit = r.CY_Adjusted_Debit,
                    CY_Adjusted_Credit = r.CY_Adjusted_Credit,
                    PY_Debit = r.PY_Debit,
                    PY_Credit = r.PY_Credit,
                    Notes = r.Notes,
                    RowVersion = r.RowVersion
                })
                .ToListAsync(ct);

            return new TB.PagedResult<TB.TrialBalanceRowDto>(items, total, q.Page, q.PageSize);
        }

        public async Task<TB.TrialBalanceRowDto?> GetAsync(int id, CancellationToken ct)
        {
            var r = await _db.TrialBalanceRows
                .AsNoTracking()
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r == null) return null;
            return new TB.TrialBalanceRowDto
            {
                Id = r.Id,
                FiscalPeriodId = r.FiscalPeriodId,
                AccountCode = r.Account.Code,
                AccountName = r.Account.Name,
                CY_Debit = r.CY_Debit,
                CY_Credit = r.CY_Credit,
                Adj_Debit = r.Adj_Debit,
                Adj_Credit = r.Adj_Credit,
                CY_Adjusted_Debit = r.CY_Adjusted_Debit,
                CY_Adjusted_Credit = r.CY_Adjusted_Credit,
                PY_Debit = r.PY_Debit,
                PY_Credit = r.PY_Credit,
                Notes = r.Notes,
                RowVersion = r.RowVersion
            };
        }

        public async Task<TB.TrialBalanceRowDto> UpdateAsync(int id, TB.TrialBalanceRowUpdateDto dto, CancellationToken ct)
        {
            if (dto.CY_Debit < 0 || dto.CY_Credit < 0 || dto.Adj_Debit < 0 || dto.Adj_Credit < 0 || dto.PY_Debit < 0 || dto.PY_Credit < 0)
                throw new ArgumentException("Amounts must be >= 0");

            var entity = await _db.TrialBalanceRows.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) throw new KeyNotFoundException();

            var period = await _db.FiscalPeriods.FirstAsync(p => p.Id == entity.FiscalPeriodId, ct);
            if (period.IsLocked) throw new InvalidOperationException("Fiscal period is locked");

            if (dto.RowVersion == null || dto.RowVersion.Length == 0) throw new ArgumentException("RowVersion is required");

            entity.CY_Debit = dto.CY_Debit;
            entity.CY_Credit = dto.CY_Credit;
            entity.Adj_Debit = dto.Adj_Debit;
            entity.Adj_Credit = dto.Adj_Credit;
            entity.PY_Debit = dto.PY_Debit;
            entity.PY_Credit = dto.PY_Credit;
            entity.Notes = dto.Notes;
            entity.CY_Adjusted_Debit = entity.CY_Debit + entity.Adj_Debit;
            entity.CY_Adjusted_Credit = entity.CY_Credit + entity.Adj_Credit;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _db.Entry(entity).Property(e => e.RowVersion).OriginalValue = dto.RowVersion;

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            var account = await _db.Accounts.AsNoTracking().FirstAsync(a => a.Id == entity.AccountId, ct);
            return new TB.TrialBalanceRowDto
            {
                Id = entity.Id,
                FiscalPeriodId = entity.FiscalPeriodId,
                AccountCode = account.Code,
                AccountName = account.Name,
                CY_Debit = entity.CY_Debit,
                CY_Credit = entity.CY_Credit,
                Adj_Debit = entity.Adj_Debit,
                Adj_Credit = entity.Adj_Credit,
                CY_Adjusted_Debit = entity.CY_Adjusted_Debit,
                CY_Adjusted_Credit = entity.CY_Adjusted_Credit,
                PY_Debit = entity.PY_Debit,
                PY_Credit = entity.PY_Credit,
                Notes = entity.Notes,
                RowVersion = entity.RowVersion
            };
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _db.TrialBalanceRows.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) throw new KeyNotFoundException();
            var period = await _db.FiscalPeriods.FirstAsync(p => p.Id == entity.FiscalPeriodId, ct);
            if (period.IsLocked) throw new InvalidOperationException("Fiscal period is locked");
            _db.TrialBalanceRows.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<TB.TrialBalanceImportResponse> ImportAsync(int fiscalPeriodId, System.IO.Stream fileStream, string fileName, bool force, CancellationToken ct)
        {
            var period = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == fiscalPeriodId, ct);
            if (period == null) throw new ArgumentException("FiscalPeriod not found");
            if (period.IsLocked && !force) throw new InvalidOperationException("Fiscal period is locked");

            if (fileStream == null) throw new ArgumentException("file is required");

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".csv")
            {
                throw new NotSupportedException("Only CSV supported in this build");
            }

            var warnings = new List<string>();
            var errors = new List<string>();
            int inserted = 0, updated = 0;

            using var reader = new StreamReader(fileStream);
            string? headerLine = await reader.ReadLineAsync();
            if (headerLine == null) throw new ArgumentException("Empty file");
            var headers = headerLine.Split(',').Select(h => h.Trim()).ToArray();
            var map = headers.Select((h, i) => (h: h.ToLowerInvariant(), i)).ToDictionary(x => x.h, x => x.i);

            decimal GetDecimal(string[] cols, string key)
            {
                if (!map.TryGetValue(key, out var idx) || idx >= cols.Length) return 0m;
                var s = cols[idx].Trim();
                if (string.IsNullOrEmpty(s)) return 0m;
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
                return 0m;
            }

            string GetString(string[] cols, string key)
            {
                if (!map.TryGetValue(key, out var idx) || idx >= cols.Length) return string.Empty;
                return cols[idx].Trim();
            }

            // Process rows
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split(',');
                var code = GetString(cols, "code");
                var name = GetString(cols, "name");
                var notes = GetString(cols, "notes");
                if (string.IsNullOrWhiteSpace(code)) { warnings.Add("Skipping row with empty Code"); continue; }

                var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Code == code, ct);
                if (account == null)
                {
                    account = new Account { Code = code, Name = string.IsNullOrWhiteSpace(name) ? code : name, IsActive = true };
                    _db.Accounts.Add(account);
                    await _db.SaveChangesAsync(ct);
                }

                var cy_d = GetDecimal(cols, "cy_debit");
                var cy_c = GetDecimal(cols, "cy_credit");
                var adj_d = GetDecimal(cols, "adj_debit");
                var adj_c = GetDecimal(cols, "adj_credit");
                var py_d = GetDecimal(cols, "py_debit");
                var py_c = GetDecimal(cols, "py_credit");

                var existing = await _db.TrialBalanceRows.FirstOrDefaultAsync(r => r.FiscalPeriodId == fiscalPeriodId && r.AccountId == account.Id, ct);
                if (existing == null)
                {
                    var row = new TrialBalanceRow
                    {
                        FiscalPeriodId = fiscalPeriodId,
                        AccountId = account.Id,
                        CY_Debit = cy_d,
                        CY_Credit = cy_c,
                        Adj_Debit = adj_d,
                        Adj_Credit = adj_c,
                        PY_Debit = py_d,
                        PY_Credit = py_c,
                        CY_Adjusted_Debit = cy_d + adj_d,
                        CY_Adjusted_Credit = cy_c + adj_c,
                        Notes = notes,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    _db.TrialBalanceRows.Add(row);
                    inserted++;
                }
                else
                {
                    existing.CY_Debit = cy_d;
                    existing.CY_Credit = cy_c;
                    existing.Adj_Debit = adj_d;
                    existing.Adj_Credit = adj_c;
                    existing.PY_Debit = py_d;
                    existing.PY_Credit = py_c;
                    existing.CY_Adjusted_Debit = cy_d + adj_d;
                    existing.CY_Adjusted_Credit = cy_c + adj_c;
                    existing.Notes = notes;
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                    updated++;
                }
            }

            await _db.SaveChangesAsync(ct);

            var summary = await SummaryAsync(fiscalPeriodId, ct);
            if (!summary.IsBalanced)
            {
                warnings.Add($"Adjusted totals not balanced: {summary.TotalAdjCY_Debit} vs {summary.TotalAdjCY_Credit}");
            }

            return new TB.TrialBalanceImportResponse(Guid.NewGuid(), "Completed", inserted, updated, warnings, errors);
        }

        public async Task<(byte[] Content, string FileName, string ContentType)> ExportAsync(int fiscalPeriodId, string format, CancellationToken ct)
        {
            var fmt = (format ?? "csv").ToLowerInvariant();
            if (fmt != "csv") throw new NotSupportedException("Only CSV supported in this build");

            var rows = await _db.TrialBalanceRows.AsNoTracking()
                .Include(r => r.Account)
                .Where(r => r.FiscalPeriodId == fiscalPeriodId)
                .OrderBy(r => r.Account.Code)
                .ToListAsync(ct);

            var csv = new List<string>();
            csv.Add("Code,Name,Level1,Level2,Level3,Level4,CY_Debit,CY_Credit,Adj_Debit,Adj_Credit,PY_Debit,PY_Credit,Notes");
            foreach (var r in rows)
            {
                string Escape(string s) => s.Contains(',') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
                csv.Add(string.Join(',',
                    Escape(r.Account.Code),
                    Escape(r.Account.Name),
                    "",
                    "",
                    "",
                    "",
                    r.CY_Debit.ToString(CultureInfo.InvariantCulture),
                    r.CY_Credit.ToString(CultureInfo.InvariantCulture),
                    r.Adj_Debit.ToString(CultureInfo.InvariantCulture),
                    r.Adj_Credit.ToString(CultureInfo.InvariantCulture),
                    r.PY_Debit.ToString(CultureInfo.InvariantCulture),
                    r.PY_Credit.ToString(CultureInfo.InvariantCulture),
                    Escape(r.Notes ?? string.Empty)
                ));
            }

            var sum = await SummaryAsync(fiscalPeriodId, ct);
            csv.Add(string.Join(',', "TOTALS", "", "", "", "", "",
                sum.TotalCY_Debit.ToString(CultureInfo.InvariantCulture),
                sum.TotalCY_Credit.ToString(CultureInfo.InvariantCulture),
                sum.TotalAdj_Debit.ToString(CultureInfo.InvariantCulture),
                sum.TotalAdj_Credit.ToString(CultureInfo.InvariantCulture),
                sum.TotalPY_Debit.ToString(CultureInfo.InvariantCulture),
                sum.TotalPY_Credit.ToString(CultureInfo.InvariantCulture),
                sum.IsBalanced ? "BALANCED" : "NOT BALANCED"));

            var content = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", csv));
            return (content, $"trialbalance_{fiscalPeriodId}.csv", "text/csv");
        }

        public async Task<TB.TrialBalanceSummaryDto> SummaryAsync(int fiscalPeriodId, CancellationToken ct)
        {
            var rows = await _db.TrialBalanceRows.AsNoTracking()
                .Where(r => r.FiscalPeriodId == fiscalPeriodId)
                .ToListAsync(ct);
            var totalCY_Debit = rows.Sum(r => r.CY_Debit);
            var totalCY_Credit = rows.Sum(r => r.CY_Credit);
            var totalAdj_Debit = rows.Sum(r => r.Adj_Debit);
            var totalAdj_Credit = rows.Sum(r => r.Adj_Credit);
            var totalAdjCY_Debit = rows.Sum(r => r.CY_Adjusted_Debit);
            var totalAdjCY_Credit = rows.Sum(r => r.CY_Adjusted_Credit);
            var totalPY_Debit = rows.Sum(r => r.PY_Debit);
            var totalPY_Credit = rows.Sum(r => r.PY_Credit);
            var balanced = totalAdjCY_Debit == totalAdjCY_Credit;
            return new TB.TrialBalanceSummaryDto(totalCY_Debit, totalCY_Credit, totalAdj_Debit, totalAdj_Credit, totalAdjCY_Debit, totalAdjCY_Credit, totalPY_Debit, totalPY_Credit, balanced);
        }

        public async Task<TB.TrialBalanceRowDto> CreateAsync(TB.TrialBalanceRowCreateDto dto, CancellationToken ct = default)
        {
            if (dto.FiscalPeriodId <= 0) throw new ArgumentException("FiscalPeriodId is required");
            if (dto.AccountId <= 0) throw new ArgumentException("AccountId is required");
            if (dto.CY_Debit < 0 || dto.CY_Credit < 0 || dto.Adj_Debit < 0 || dto.Adj_Credit < 0 || dto.PY_Debit < 0 || dto.PY_Credit < 0)
                throw new ArgumentException("Amounts must be >= 0");

            var period = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == dto.FiscalPeriodId, ct)
                         ?? throw new ArgumentException("FiscalPeriod not found");
            if (period.IsLocked) throw new InvalidOperationException("Fiscal period is locked");

            // Idempotency/uniqueness: one row per (period, account)
            var exists = await _db.TrialBalanceRows
                .AnyAsync(r => r.FiscalPeriodId == dto.FiscalPeriodId && r.AccountId == dto.AccountId, ct);
            if (exists) throw new InvalidOperationException("Row already exists for this FiscalPeriodId + AccountId");

            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == dto.AccountId, ct)
                         ?? throw new ArgumentException("Account not found");

            var row = new TrialBalanceRow
            {
                FiscalPeriodId = dto.FiscalPeriodId,
                AccountId = dto.AccountId,
                CY_Debit = dto.CY_Debit,
                CY_Credit = dto.CY_Credit,
                Adj_Debit = dto.Adj_Debit,
                Adj_Credit = dto.Adj_Credit,
                PY_Debit = dto.PY_Debit,
                PY_Credit = dto.PY_Credit,
                CY_Adjusted_Debit = dto.CY_Debit + dto.Adj_Debit,
                CY_Adjusted_Credit = dto.CY_Credit + dto.Adj_Credit,
                Notes = dto.Notes,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.TrialBalanceRows.Add(row);
            await _db.SaveChangesAsync(ct);

            // Return DTO (load account for names/codes)
            return new TB.TrialBalanceRowDto
            {
                Id = row.Id,
                FiscalPeriodId = row.FiscalPeriodId,
                AccountCode = account.Code,
                AccountName = account.Name,
                CY_Debit = row.CY_Debit,
                CY_Credit = row.CY_Credit,
                Adj_Debit = row.Adj_Debit,
                Adj_Credit = row.Adj_Credit,
                CY_Adjusted_Debit = row.CY_Adjusted_Debit,
                CY_Adjusted_Credit = row.CY_Adjusted_Credit,
                PY_Debit = row.PY_Debit,
                PY_Credit = row.PY_Credit,
                Notes = row.Notes,
                RowVersion = row.RowVersion
            };
        }

        public async Task<TB.TrialBalanceRowDto> CreateByCodeAsync(TB.TrialBalanceRowCreateByCodeDto dto, CancellationToken ct = default)
        {
            if (dto.FiscalPeriodId <= 0) throw new ArgumentException("FiscalPeriodId is required");
            if (string.IsNullOrWhiteSpace(dto.AccountCode)) throw new ArgumentException("AccountCode is required");
            if (dto.CY_Debit < 0 || dto.CY_Credit < 0 || dto.Adj_Debit < 0 || dto.Adj_Credit < 0 || dto.PY_Debit < 0 || dto.PY_Credit < 0)
                throw new ArgumentException("Amounts must be >= 0");

            var period = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == dto.FiscalPeriodId, ct)
                         ?? throw new ArgumentException("FiscalPeriod not found");
            if (period.IsLocked) throw new InvalidOperationException("Fiscal period is locked");

            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Code == dto.AccountCode, ct);
            if (account == null)
            {
                if (!dto.AutoCreateAccountIfMissing)
                    throw new ArgumentException("Account not found by code");
                account = new Account { Code = dto.AccountCode, Name = dto.AccountCode, IsActive = true };
                _db.Accounts.Add(account);
                await _db.SaveChangesAsync(ct);
            }

            var exists = await _db.TrialBalanceRows
                .AnyAsync(r => r.FiscalPeriodId == dto.FiscalPeriodId && r.AccountId == account.Id, ct);
            if (exists) throw new InvalidOperationException("Row already exists for this FiscalPeriodId + AccountCode");

            var row = new TrialBalanceRow
            {
                FiscalPeriodId = dto.FiscalPeriodId,
                AccountId = account.Id,
                CY_Debit = dto.CY_Debit,
                CY_Credit = dto.CY_Credit,
                Adj_Debit = dto.Adj_Debit,
                Adj_Credit = dto.Adj_Credit,
                PY_Debit = dto.PY_Debit,
                PY_Credit = dto.PY_Credit,
                CY_Adjusted_Debit = dto.CY_Debit + dto.Adj_Debit,
                CY_Adjusted_Credit = dto.CY_Credit + dto.Adj_Credit,
                Notes = dto.Notes,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.TrialBalanceRows.Add(row);
            await _db.SaveChangesAsync(ct);

            return new TB.TrialBalanceRowDto
            {
                Id = row.Id,
                FiscalPeriodId = row.FiscalPeriodId,
                AccountCode = account.Code,
                AccountName = account.Name,
                CY_Debit = row.CY_Debit,
                CY_Credit = row.CY_Credit,
                Adj_Debit = row.Adj_Debit,
                Adj_Credit = row.Adj_Credit,
                CY_Adjusted_Debit = row.CY_Adjusted_Debit,
                CY_Adjusted_Credit = row.CY_Adjusted_Credit,
                PY_Debit = row.PY_Debit,
                PY_Credit = row.PY_Credit,
                Notes = row.Notes,
                RowVersion = row.RowVersion
            };
        }

    }
}
