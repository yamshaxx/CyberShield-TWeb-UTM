using CyberShield.Domain.Model.Blog;
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
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }

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