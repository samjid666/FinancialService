using Microsoft.EntityFrameworkCore;
using NftApi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NftApi.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<FinancialRecord> FinancialRecords { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasIndex(p => new { p.FirstName, p.Surname, p.DateOfBirth });
            });

            modelBuilder.Entity<FinancialRecord>(entity =>
            {
                entity.HasIndex(f => f.TransactionDate);
                entity.HasIndex(f => f.Status);
                entity.HasIndex(f => new { f.PersonId, f.Status });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PasswordHash).IsRequired();
            });
        }
    }
}
