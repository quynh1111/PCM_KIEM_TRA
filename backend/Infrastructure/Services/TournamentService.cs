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
        private readonly IWalletService _walletService;

        public TournamentService(IUnitOfWork unitOfWork, IWalletService walletService)
        {
            _unitOfWork = unitOfWork;
            _walletService = walletService;
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

        public async Task<TournamentDto?> UpdateTournamentAsync(int id, CreateTournamentDto dto)
        {
            var tournament = await _unitOfWork.Tournaments.GetByIdAsync(id);
            if (tournament == null)
                return null;

            tournament.Name = dto.Name;
            tournament.Description = dto.Description;
            tournament.StartDate = dto.StartDate;
            tournament.EndDate = dto.EndDate;
            tournament.Type = dto.Type;
            tournament.Format = dto.Format;
            tournament.EntryFee = dto.EntryFee;
            tournament.PrizePool = dto.PrizePool;
            tournament.MaxParticipants = dto.MaxParticipants;
            tournament.RegistrationDeadline = dto.RegistrationDeadline;

            _unitOfWork.Tournaments.Update(tournament);
            await _unitOfWork.SaveChangesAsync();

            var count = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == id)).Count();
            return MapToDto(tournament, count);
        }

        public async Task<bool> JoinTournamentAsync(int tournamentId, string userId, string? teamName = null)
        {
            var tournament = await _unitOfWork.Tournaments.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new Exception("Không tìm thấy giải đấu");

            if (tournament.Status != TournamentStatus.Open)
                throw new Exception("Giải đấu chưa mở đăng ký");

            if (tournament.RegistrationDeadline.HasValue)
            {
                var deadline = tournament.RegistrationDeadline.Value;
                if (deadline.TimeOfDay == TimeSpan.Zero)
                {
                    deadline = deadline.Date.AddDays(1).AddTicks(-1);
                }

                if (deadline < DateTime.UtcNow)
                    throw new Exception("Đã hết hạn đăng ký");
            }

            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Không tìm thấy hội viên");

            var existing = (await _unitOfWork.Participants.FindAsync(p =>
                p.TournamentId == tournamentId && p.MemberId == member.Id)).FirstOrDefault();
            if (existing != null)
                throw new Exception("Bạn đã đăng ký giải này");

            var currentCount = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == tournamentId)).Count();
            if (tournament.MaxParticipants > 0 && currentCount >= tournament.MaxParticipants)
                throw new Exception("Giải đấu đã đủ người tham gia");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (tournament.EntryFee > 0)
                {
                    var balance = await _walletService.GetBalanceAsync(userId);
                    member.WalletBalance = balance;

                    if (member.WalletBalance < tournament.EntryFee)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw new Exception("Số dư ví không đủ");
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
            if (tournamentMatches.Count == 0)
                return null;
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

            if (participants.Count < 2)
                return false;

            var bracketSize = 1;
            while (bracketSize < participants.Count)
            {
                bracketSize <<= 1;
            }

            var rounds = new List<List<TournamentMatch>>();
            var totalRounds = (int)Math.Log2(bracketSize);
            var seeded = participants.Select(p => p.MemberId).ToList();
            while (seeded.Count < bracketSize)
            {
                seeded.Add(0);
            }

            for (var round = 1; round <= totalRounds; round++)
            {
                var matchCount = bracketSize / (int)Math.Pow(2, round);
                var roundMatches = new List<TournamentMatch>();

                for (var position = 1; position <= matchCount; position++)
                {
                    Match match;
                    if (round == 1)
                    {
                        var pairIndex = (position - 1) * 2;
                        var team1Id = seeded[pairIndex];
                        var team2Id = seeded[pairIndex + 1];
                        match = new Match
                        {
                            TournamentId = tournamentId,
                            Date = DateTime.UtcNow.AddDays(1),
                            IsRanked = false,
                            MatchFormat = MatchFormat.Singles,
                            Team1Player1Id = team1Id,
                            Team2Player1Id = team2Id,
                            Status = MatchStatus.Scheduled,
                            Result = WinnerSide.None
                        };

                        if (team1Id == 0 && team2Id == 0)
                        {
                            match.Status = MatchStatus.Completed;
                        }
                        else if (team1Id == 0 || team2Id == 0)
                        {
                            match.Status = MatchStatus.Completed;
                            match.Result = team1Id == 0 ? WinnerSide.Team2 : WinnerSide.Team1;
                            match.ScoreTeam1 = team1Id == 0 ? 0 : 1;
                            match.ScoreTeam2 = team2Id == 0 ? 0 : 1;
                        }
                    }
                    else
                    {
                        match = new Match
                        {
                            TournamentId = tournamentId,
                            Date = DateTime.UtcNow.AddDays(round),
                            IsRanked = false,
                            MatchFormat = MatchFormat.Singles,
                            Team1Player1Id = 0,
                            Team2Player1Id = 0,
                            Status = MatchStatus.Scheduled,
                            Result = WinnerSide.None
                        };
                    }

                    await _unitOfWork.Matches.AddAsync(match);
                    await _unitOfWork.SaveChangesAsync();

                    var tournamentMatch = new TournamentMatch
                    {
                        TournamentId = tournamentId,
                        MatchId = match.Id,
                        Round = round,
                        Position = position,
                        BracketGroup = "WinnerBracket"
                    };

                    await _unitOfWork.TournamentMatches.AddAsync(tournamentMatch);
                    await _unitOfWork.SaveChangesAsync();
                    roundMatches.Add(tournamentMatch);
                }

                rounds.Add(roundMatches);
            }

            for (var roundIndex = 0; roundIndex < rounds.Count - 1; roundIndex++)
            {
                var currentRound = rounds[roundIndex];
                var nextRound = rounds[roundIndex + 1];

                for (var i = 0; i < currentRound.Count; i++)
                {
                    var next = nextRound[i / 2];
                    currentRound[i].NextMatchId = next.Id;
                    _unitOfWork.TournamentMatches.Update(currentRound[i]);
                }
            }

            if (rounds.Count > 1)
            {
                var firstRound = rounds[0];
                foreach (var tm in firstRound)
                {
                    var match = await _unitOfWork.Matches.GetByIdAsync(tm.MatchId);
                    if (match == null || match.Status != MatchStatus.Completed)
                        continue;

                    if (!tm.NextMatchId.HasValue)
                        continue;

                    var nextTournamentMatch = await _unitOfWork.TournamentMatches.GetByIdAsync(tm.NextMatchId.Value);
                    if (nextTournamentMatch == null)
                        continue;

                    var nextMatch = await _unitOfWork.Matches.GetByIdAsync(nextTournamentMatch.MatchId);
                    if (nextMatch == null)
                        continue;

                    var team1Winner = match.Result == WinnerSide.Team1;
                    var winner1 = team1Winner ? match.Team1Player1Id : match.Team2Player1Id;
                    var winner2 = team1Winner ? match.Team1Player2Id : match.Team2Player2Id;

                    if (winner1 == 0)
                        continue;

                    if (tm.Position % 2 == 1)
                    {
                        nextMatch.Team1Player1Id = winner1;
                        nextMatch.Team1Player2Id = winner2;
                    }
                    else
                    {
                        nextMatch.Team2Player1Id = winner1;
                        nextMatch.Team2Player2Id = winner2;
                    }

                    _unitOfWork.Matches.Update(nextMatch);
                }
            }

            tournament.Status = TournamentStatus.Ongoing;
            _unitOfWork.Tournaments.Update(tournament);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AutoDivideTeamsAsync(int tournamentId)
        {
            var participants = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == tournamentId)).ToList();
            if (!participants.Any())
                return false;

            var members = await _unitOfWork.Members.GetAllAsync();
            var rankMap = members.ToDictionary(m => m.Id, m => m.RankELO);

            var ordered = participants
                .OrderByDescending(p => rankMap.TryGetValue(p.MemberId, out var elo) ? elo : 0)
                .ThenBy(p => p.RegisteredDate)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].TeamName = i % 2 == 0 ? "Team A" : "Team B";
                _unitOfWork.Participants.Update(ordered[i]);
            }

            await _unitOfWork.SaveChangesAsync();
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
