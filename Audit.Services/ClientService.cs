using Audit.Services.Interfaces;
using AuditPilot.Data;
using AuditPilot.Data.Entities;
using AuthPilot.Models.Clients;
using Microsoft.EntityFrameworkCore;

namespace Audit.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _db;

        public ClientService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ClientDto?> CreateAsync(CreateClientModel model, CancellationToken ct = default)
        {
            var entity = new Client
            {
                Name = model.Name,
                Email = model.Email,
                CompanyType = model.CompanyType,
                CompanyName = model.CompanyName
            };
            _db.Clients.Add(entity);
            await _db.SaveChangesAsync(ct);
            return await MapToDto(entity.Id, ct);
        }

        public async Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
            return entity == null ? null : Map(entity);
        }

        public async Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Clients
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ClientDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    CompanyType = c.CompanyType,
                    CompanyName = c.CompanyName,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(ct);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateClientModel model, CancellationToken ct = default)
        {
            var entity = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity == null) return false;
            entity.Name = model.Name;
            entity.Email = model.Email;
            entity.CompanyType = model.CompanyType;
            entity.CompanyName = model.CompanyName;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity == null) return false;
            _db.Clients.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private async Task<ClientDto?> MapToDto(Guid id, CancellationToken ct)
        {
            var entity = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
            return entity == null ? null : Map(entity);
        }

        private static ClientDto Map(Client c) => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            CompanyType = c.CompanyType,
            CompanyName = c.CompanyName,
            CreatedAt = c.CreatedAt
        };
    }
}

