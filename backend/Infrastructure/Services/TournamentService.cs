using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PCM.Application.DTOs.Tournaments;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TournamentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TournamentDto>> GetAllTournamentsAsync()
        {
            var tournaments = await _unitOfWork.Tournaments.GetAllAsync();
            var participants = await _unitOfWork.Participants.GetAllAsync();
            var counts = participants
                .GroupBy(p => p.TournamentId)
                .ToDictionary(g => g.Key, g => g.Count());

            return tournaments
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => MapToDto(t, counts.TryGetValue(t.Id, out var count) ? count : 0))
                .ToList();
        }

        public async Task<TournamentDto?> GetByIdAsync(int id)
        {
            var tournament = await _unitOfWork.Tournaments.GetByIdAsync(id);
            if (tournament == null)
                return null;

            var count = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == id)).Count();
            return MapToDto(tournament, count);
        }

        public async Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto, string userId)
        {
            var tournament = new Tournament
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Type = dto.Type,
                Format = dto.Format,
                Status = TournamentStatus.Open,
                EntryFee = dto.EntryFee,
                PrizePool = dto.PrizePool,
                MaxParticipants = dto.MaxParticipants,
                RegistrationDeadline = dto.RegistrationDeadline,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Tournaments.AddAsync(tournament);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(tournament, 0);
        }

        public async Task<bool> JoinTournamentAsync(int tournamentId, string userId, string? teamName = null)
        {
            var tournament = await _unitOfWork.Tournaments.GetByIdAsync(tournamentId);
            if (tournament == null)
                return false;

            if (tournament.Status != TournamentStatus.Open)
                return false;

            if (tournament.RegistrationDeadline.HasValue && tournament.RegistrationDeadline.Value < DateTime.UtcNow)
                return false;

            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                return false;

            var existing = (await _unitOfWork.Participants.FindAsync(p =>
                p.TournamentId == tournamentId && p.MemberId == member.Id)).FirstOrDefault();
            if (existing != null)
                return false;

            var currentCount = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == tournamentId)).Count();
            if (tournament.MaxParticipants > 0 && currentCount >= tournament.MaxParticipants)
                return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (tournament.EntryFee > 0)
                {
                    if (member.WalletBalance < tournament.EntryFee)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return false;
                    }

                    var category = await EnsureWalletCategoryAsync("PayTournament", TransactionType.Expense, TransactionScope.Wallet);

                    member.WalletBalance -= tournament.EntryFee;
                    _unitOfWork.Members.Update(member);

                    var transaction = new WalletTransaction
                    {
                        MemberId = member.Id,
                        Date = DateTime.UtcNow,
                        Amount = -tournament.EntryFee,
                        CategoryId = category.Id,
                        Type = WalletTransactionType.PayTournament,
                        ReferenceId = $"TOURNAMENT-{tournament.Id}",
                        Description = $"Đăng ký giải đấu {tournament.Name}",
                        Status = WalletTransactionStatus.Success
                    };

                    await _unitOfWork.WalletTransactions.AddAsync(transaction);
                }

                var participant = new Participant
                {
                    TournamentId = tournamentId,
                    MemberId = member.Id,
                    TeamName = teamName ?? member.FullName,
                    Status = tournament.EntryFee > 0 ? ParticipantStatus.Paid : ParticipantStatus.Registered,
                    RegisteredDate = DateTime.UtcNow
                };

                await _unitOfWork.Participants.AddAsync(participant);
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<BracketDto?> GetBracketAsync(int tournamentId)
        {
            var tournament = await _unitOfWork.Tournaments.GetByIdAsync(tournamentId);
            if (tournament == null)
                return null;

            var tournamentMatches = (await _unitOfWork.TournamentMatches.FindAsync(tm => tm.TournamentId == tournamentId)).ToList();
            var matches = (await _unitOfWork.Matches.FindAsync(m => m.TournamentId == tournamentId)).ToDictionary(m => m.Id);
            var members = (await _unitOfWork.Members.GetAllAsync()).ToDictionary(m => m.Id, m => m.FullName);

            var nodes = new List<BracketNode>();
            var edges = new List<BracketEdge>();

            foreach (var tm in tournamentMatches.OrderBy(t => t.Round).ThenBy(t => t.Position))
            {
                matches.TryGetValue(tm.MatchId, out var match);

                var team1 = match != null && members.TryGetValue(match.Team1Player1Id, out var t1) ? t1 : null;
                var team2 = match != null && members.TryGetValue(match.Team2Player1Id, out var t2) ? t2 : null;

                var winner = match?.Result == WinnerSide.Team1 ? team1 :
                             match?.Result == WinnerSide.Team2 ? team2 : null;

                nodes.Add(new BracketNode
                {
                    Id = tm.Id,
                    MatchId = tm.MatchId,
                    Round = tm.Round,
                    Position = tm.Position,
                    Team1 = team1,
                    Team2 = team2,
                    ScoreTeam1 = match?.ScoreTeam1,
                    ScoreTeam2 = match?.ScoreTeam2,
                    Winner = winner,
                    Status = match?.Status.ToString() ?? MatchStatus.Scheduled.ToString()
                });

                if (tm.NextMatchId.HasValue)
                {
                    edges.Add(new BracketEdge { From = tm.Id, To = tm.NextMatchId.Value });
                }
            }

            return new BracketDto
            {
                TournamentId = tournament.Id,
                TournamentName = tournament.Name,
                Nodes = nodes,
                Edges = edges
            };
        }

        public async Task<bool> GenerateBracketAsync(int tournamentId)
        {
            var tournament = await _unitOfWork.Tournaments.GetByIdAsync(tournamentId);
            if (tournament == null)
                return false;

            var existing = await _unitOfWork.TournamentMatches.FindAsync(tm => tm.TournamentId == tournamentId);
            if (existing.Any())
                return true;

            var participants = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == tournamentId))
                .OrderBy(p => p.SeedNo ?? int.MaxValue)
                .ThenBy(p => p.RegisteredDate)
                .ToList();

            if (participants.Count < 2 || participants.Count % 2 != 0)
                return false;

            var round = 1;
            var position = 1;
            foreach (var pair in participants.Chunk(2))
            {
                var match = new Match
                {
                    TournamentId = tournamentId,
                    Date = DateTime.UtcNow.AddDays(1),
                    IsRanked = false,
                    MatchFormat = MatchFormat.Singles,
                    Team1Player1Id = pair[0].MemberId,
                    Team2Player1Id = pair[1].MemberId,
                    Status = MatchStatus.Scheduled,
                    Result = WinnerSide.None
                };

                await _unitOfWork.Matches.AddAsync(match);
                await _unitOfWork.SaveChangesAsync();

                var tournamentMatch = new TournamentMatch
                {
                    TournamentId = tournamentId,
                    MatchId = match.Id,
                    Round = round,
                    Position = position++,
                    BracketGroup = "WinnerBracket"
                };

                await _unitOfWork.TournamentMatches.AddAsync(tournamentMatch);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        private async Task<TransactionCategory> EnsureWalletCategoryAsync(string name, TransactionType type, TransactionScope scope)
        {
            var existing = (await _unitOfWork.TransactionCategories.FindAsync(c =>
                c.Name == name && c.Type == type.ToString() && c.Scope == scope.ToString()))
                .FirstOrDefault();

            if (existing != null)
                return existing;

            var category = new TransactionCategory
            {
                Id = Guid.NewGuid(),
                Name = name,
                Type = type.ToString(),
                Scope = scope.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TransactionCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        private static TournamentDto MapToDto(Tournament tournament, int participantCount)
        {
            return new TournamentDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Description = tournament.Description,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Type = tournament.Type.ToString(),
                Format = tournament.Format.ToString(),
                Status = tournament.Status.ToString(),
                EntryFee = tournament.EntryFee,
                PrizePool = tournament.PrizePool,
                MaxParticipants = tournament.MaxParticipants,
                CurrentParticipants = participantCount,
                RegistrationDeadline = tournament.RegistrationDeadline,
                CreatedDate = tournament.CreatedDate,
                CreatedBy = tournament.CreatedBy
            };
        }
    }
}
