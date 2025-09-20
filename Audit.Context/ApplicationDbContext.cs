using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuditPilot.Data.Entities;

namespace AuditPilot.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<ShareHolder> ShareHolders { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; } = null!;
        public DbSet<FiscalPeriod> FiscalPeriods { get; set; } = null!;
        public DbSet<TrialBalanceRow> TrialBalanceRows { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Account>()
                .HasIndex(a => a.Code)
                .IsUnique();

            builder.Entity<Account>()
                .HasOne(a => a.Parent)
                .WithMany()
                .HasForeignKey(a => a.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JournalEntryLine>()
                .HasOne(l => l.JournalEntry)
                .WithMany(j => j.Lines)
                .HasForeignKey(l => l.JournalEntryId);

            builder.Entity<JournalEntryLine>()
                .HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId);

            builder.Entity<TrialBalanceRow>()
                .HasIndex(r => new { r.FiscalPeriodId, r.AccountId })
                .IsUnique();

            builder.Entity<TrialBalanceRow>()
                .Property(r => r.RowVersion)
                .IsRowVersion();

            // Decimal precision enforcement (SQL Server style)
            builder.Entity<TrialBalanceRow>().Property(r => r.CY_Debit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.CY_Credit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.Adj_Debit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.Adj_Credit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.CY_Adjusted_Debit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.CY_Adjusted_Credit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.PY_Debit).HasPrecision(18, 2);
            builder.Entity<TrialBalanceRow>().Property(r => r.PY_Credit).HasPrecision(18, 2);
        }
    }
}
