using CyberShield.Domain.Model.User;
using System.Data.Entity;

namespace CyberShield.Domain.Data
{
    public class CyberShieldContext : DbContext
    {
        public CyberShieldContext() : base("name=CyberShieldConnection")
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique(true);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(true);
        }
    }
} 