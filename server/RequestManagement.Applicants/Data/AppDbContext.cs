using Microsoft.EntityFrameworkCore;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Request> Requests => Set<Request>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Request>()
                .HasIndex(r => new { r.Status, r.Priority, r.CreatedAt });

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.OrganizationName);

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.AssignedTo);

            modelBuilder.Entity<Request>()
            .HasIndex(r => r.CreatedAt);

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.Priority);
        }
    }
}