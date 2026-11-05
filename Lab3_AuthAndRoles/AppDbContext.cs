using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public class AppDbContext : DbContext
    {
        public DbSet<Computer> Computers { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString =
                    "Host=aws-0-ap-northeast-1.pooler.supabase.com;" +
                    "Port=5432;" +
                    "Database=postgres;" +
                    "Username=postgres.evarfjzephdnmnbeyqbu;" +
                    "Password=e18rta2007twitch;" +
                    "SSL Mode=Require;" +
                    "Trust Server Certificate=true";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Computer>()
                .HasMany(c => c.Sessions)
                .WithOne(s => s.Computer)
                .HasForeignKey(s => s.ComputerId);
        }
    }
}

