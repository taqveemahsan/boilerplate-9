using Audit.Services.Interfaces;
using AuditPilot.Data;
using AuditPilot.Data.Entities;
using AuthPilot.Models.ShareHolders;
using Microsoft.EntityFrameworkCore;

namespace Audit.Services
{
    public class ShareHolderService : IShareHolderService
    {
        private readonly ApplicationDbContext _db;

        public ShareHolderService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<ShareHolderDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.ShareHolders.AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ShareHolderDto
                {
                    Id = s.Id,
                    Email = s.Email,
                    Cnic = s.Cnic,
                    CompanyName = s.CompanyName,
                    Name = s.Name,
                    Percentage = s.Percentage,
                    Reason = s.Reason,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync(ct);
        }

        public async Task<ShareHolderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var s = await _db.ShareHolders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return s == null ? null : Map(s);
        }

        public async Task<ShareHolderDto?> CreateAsync(CreateShareHolderModel model, CancellationToken ct = default)
        {
            var entity = new ShareHolder
            {
                Email = model.Email,
                Cnic = model.Cnic,
                CompanyName = model.CompanyName,
                Name = model.Name,
                Percentage = model.Percentage,
                Reason = model.Reason
            };
            _db.ShareHolders.Add(entity);
            await _db.SaveChangesAsync(ct);
            return Map(entity);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateShareHolderModel model, CancellationToken ct = default)
        {
            var entity = await _db.ShareHolders.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity == null) return false;
            entity.Email = model.Email;
            entity.Cnic = model.Cnic;
            entity.CompanyName = model.CompanyName;
            entity.Name = model.Name;
            entity.Percentage = model.Percentage;
            entity.Reason = model.Reason;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.ShareHolders.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity == null) return false;
            _db.ShareHolders.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private static ShareHolderDto Map(ShareHolder s) => new ShareHolderDto
        {
            Id = s.Id,
            Email = s.Email,
            Cnic = s.Cnic,
            CompanyName = s.CompanyName,
            Name = s.Name,
            Percentage = s.Percentage,
            Reason = s.Reason,
            CreatedAt = s.CreatedAt
        };
    }
}
