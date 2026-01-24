using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PCM.Domain.Entities;
using System.Reflection;

namespace PCM.Infrastructure.Persistence
{
    /// <summary>
    /// Main database context
    /// IMPORTANT: Replace XXX with last 3 digits of your student ID in table names
    /// </summary>
    public class PCMDbContext : IdentityDbContext
    {
        public PCMDbContext(DbContextOptions<PCMDbContext> options) : base(options)
        {
        }

        // Members & Identity
        public DbSet<Member> Members { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<News> News { get; set; }

        // Treasury & Wallet
        public DbSet<TransactionCategory> TransactionCategories { get; set; }
        public DbSet<TreasuryTransaction> TreasuryTransactions { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        // Booking System
        public DbSet<Court> Courts { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // Tournament System
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<TournamentMatch> TournamentMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations from assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
