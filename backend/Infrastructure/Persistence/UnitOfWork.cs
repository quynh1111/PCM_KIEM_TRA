using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PCMDbContext _context;
        private IDbContextTransaction? _transaction;
        private int _transactionDepth;

        private IRepository<Member>? _members;
        private IRepository<RefreshToken>? _refreshTokens;
        private IRepository<News>? _news;
        private IRepository<TransactionCategory>? _transactionCategories;
        private IRepository<TreasuryTransaction>? _treasuryTransactions;
        private IRepository<WalletTransaction>? _walletTransactions;
        private IRepository<Court>? _courts;
        private IRepository<Booking>? _bookings;
        private IRepository<Tournament>? _tournaments;
        private IRepository<Participant>? _participants;
        private IRepository<Match>? _matches;
        private IRepository<TournamentMatch>? _tournamentMatches;

        public UnitOfWork(PCMDbContext context)
        {
            _context = context;
        }

        public IRepository<Member> Members 
            => _members ??= new Repository<Member>(_context);

        public IRepository<RefreshToken> RefreshTokens 
            => _refreshTokens ??= new Repository<RefreshToken>(_context);

        public IRepository<News> News 
            => _news ??= new Repository<News>(_context);

        public IRepository<TransactionCategory> TransactionCategories 
            => _transactionCategories ??= new Repository<TransactionCategory>(_context);

        public IRepository<TreasuryTransaction> TreasuryTransactions 
            => _treasuryTransactions ??= new Repository<TreasuryTransaction>(_context);

        public IRepository<WalletTransaction> WalletTransactions 
            => _walletTransactions ??= new Repository<WalletTransaction>(_context);

        public IRepository<Court> Courts 
            => _courts ??= new Repository<Court>(_context);

        public IRepository<Booking> Bookings 
            => _bookings ??= new Repository<Booking>(_context);

        public IRepository<Tournament> Tournaments 
            => _tournaments ??= new Repository<Tournament>(_context);

        public IRepository<Participant> Participants 
            => _participants ??= new Repository<Participant>(_context);

        public IRepository<Match> Matches 
            => _matches ??= new Repository<Match>(_context);

        public IRepository<TournamentMatch> TournamentMatches 
            => _tournamentMatches ??= new Repository<TournamentMatch>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transactionDepth == 0)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
            _transactionDepth++;
        }

        public async Task CommitTransactionAsync()
        {
            if (_transactionDepth == 0)
                return;

            try
            {
                await SaveChangesAsync();

                _transactionDepth--;
                if (_transactionDepth == 0 && _transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transactionDepth == 0 && _transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            _transactionDepth = 0;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
