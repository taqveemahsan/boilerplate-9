using Audit.Services.Interfaces;
using AuditPilot.Data;
using AuditPilot.Data.Entities;
using AuthPilot.Models.FiscalPeriods;
using Microsoft.EntityFrameworkCore;

namespace Audit.Services
{
    public class FiscalPeriodService : IFiscalPeriodService
    {
        private const int MaxPageSize = 500;
        private readonly ApplicationDbContext _db;

        public FiscalPeriodService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<FiscalPeriodDto>> ListAsync(FiscalPeriodQuery query, CancellationToken ct)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 50 : Math.Min(query.PageSize, MaxPageSize);

            var baseQuery = _db.FiscalPeriods.AsNoTracking().AsQueryable();

            if (query.ClientId.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.ClientId == query.ClientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(p => p.Name.ToLower().Contains(search));
            }

            var total = await baseQuery.CountAsync(ct);
            var items = await baseQuery
                .OrderByDescending(p => p.StartDate)
                .ThenBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new FiscalPeriodDto
                {
                    Id = p.Id,
                    ClientId = p.ClientId,
                    Name = p.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsLocked = p.IsLocked,
                    CreatedAtUtc = p.CreatedAtUtc
                })
                .ToListAsync(ct);

            return new PagedResult<FiscalPeriodDto>(items, total, page, pageSize);
        }

        public async Task<FiscalPeriodDto?> GetAsync(int id, CancellationToken ct)
        {
            var entity = await _db.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<FiscalPeriodDto> CreateAsync(FiscalPeriodCreateRequest request, CancellationToken ct)
        {
            ValidateDateRange(request.StartDate, request.EndDate);
            await EnsureClientExists(request.ClientId, ct);
            await EnsureNameUnique(request.ClientId, request.Name, null, ct);
            await EnsureNoOverlap(request.ClientId, request.StartDate, request.EndDate, null, ct);

            var entity = new FiscalPeriod
            {
                ClientId = request.ClientId,
                Name = request.Name.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsLocked = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.FiscalPeriods.Add(entity);
            await _db.SaveChangesAsync(ct);

            return Map(entity);
        }

        public async Task<FiscalPeriodDto> UpdateAsync(int id, FiscalPeriodUpdateRequest request, CancellationToken ct)
        {
            ValidateDateRange(request.StartDate, request.EndDate);

            var entity = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException("Fiscal period not found");
            }

            if (entity.IsLocked)
            {
                throw new InvalidOperationException("Fiscal period is locked");
            }

            if (entity.ClientId != request.ClientId)
            {
                await EnsureClientExists(request.ClientId, ct);
                var hasRows = await _db.TrialBalanceRows.AnyAsync(r => r.FiscalPeriodId == entity.Id, ct);
                if (hasRows)
                {
                    throw new InvalidOperationException("Cannot change client for a fiscal period with trial balance data");
                }
            }

            await EnsureNameUnique(request.ClientId, request.Name, id, ct);
            await EnsureNoOverlap(request.ClientId, request.StartDate, request.EndDate, id, ct);

            entity.ClientId = request.ClientId;
            entity.Name = request.Name.Trim();
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;

            await _db.SaveChangesAsync(ct);

            return Map(entity);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException("Fiscal period not found");
            }

            if (entity.IsLocked)
            {
                throw new InvalidOperationException("Cannot delete a locked fiscal period");
            }

            var hasRows = await _db.TrialBalanceRows.AnyAsync(r => r.FiscalPeriodId == id, ct);
            if (hasRows)
            {
                throw new InvalidOperationException("Cannot delete fiscal period with trial balance data");
            }

            _db.FiscalPeriods.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<FiscalPeriodDto> LockAsync(int id, CancellationToken ct)
        {
            var entity = await _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException("Fiscal period not found");
            }

            if (entity.IsLocked)
            {
                return Map(entity);
            }

            entity.IsLocked = true;
            await _db.SaveChangesAsync(ct);

            return Map(entity);
        }

        private static void ValidateDateRange(DateOnly start, DateOnly end)
        {
            if (start > end)
            {
                throw new ArgumentException("startDate must be before or equal to endDate");
            }
        }

        private async Task EnsureClientExists(Guid clientId, CancellationToken ct)
        {
            var exists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
            if (!exists)
            {
                throw new ArgumentException("Client not found");
            }
        }

        private async Task EnsureNameUnique(Guid clientId, string name, int? excludeId, CancellationToken ct)
        {
            var trimmed = name.Trim();
            var query = _db.FiscalPeriods.AsNoTracking().Where(p => p.ClientId == clientId && p.Name == trimmed);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(ct);
            if (exists)
            {
                throw new InvalidOperationException("Fiscal period name must be unique per client");
            }
        }

        private async Task EnsureNoOverlap(Guid clientId, DateOnly start, DateOnly end, int? excludeId, CancellationToken ct)
        {
            var query = _db.FiscalPeriods.AsNoTracking()
                .Where(p => p.ClientId == clientId)
                .Where(p => p.StartDate <= end && start <= p.EndDate);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            var overlaps = await query.AnyAsync(ct);
            if (overlaps)
            {
                throw new InvalidOperationException("Fiscal period dates overlap with an existing period");
            }
        }

        private static FiscalPeriodDto Map(FiscalPeriod entity) => new()
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsLocked = entity.IsLocked,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
