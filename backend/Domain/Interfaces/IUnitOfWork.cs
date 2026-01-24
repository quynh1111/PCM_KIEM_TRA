using System;
using System.Threading.Tasks;
using PCM.Domain.Entities;

namespace PCM.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern for transaction management
    /// CRITICAL: Ensures data integrity for wallet and booking operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Member> Members { get; }
        IRepository<RefreshToken> RefreshTokens { get; }
        IRepository<News> News { get; }
        IRepository<TransactionCategory> TransactionCategories { get; }
        IRepository<TreasuryTransaction> TreasuryTransactions { get; }
        IRepository<WalletTransaction> WalletTransactions { get; }
        IRepository<Court> Courts { get; }
        IRepository<Booking> Bookings { get; }
        IRepository<Tournament> Tournaments { get; }
        IRepository<Participant> Participants { get; }
        IRepository<Match> Matches { get; }
        IRepository<TournamentMatch> TournamentMatches { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
