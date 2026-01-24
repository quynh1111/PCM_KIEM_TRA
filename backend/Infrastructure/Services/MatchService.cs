using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PCM.Application.DTOs.Matches;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class MatchService : IMatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEloRatingService _eloRatingService;
        private readonly ICacheService _cacheService;

        public MatchService(IUnitOfWork unitOfWork, IEloRatingService eloRatingService, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _eloRatingService = eloRatingService;
            _cacheService = cacheService;
        }

        public async Task<MatchDto> CreateMatchAsync(CreateMatchDto dto)
        {
            var match = new Match
            {
                Date = DateTime.UtcNow,
                IsRanked = dto.IsRanked,
                MatchFormat = dto.MatchFormat,
                Team1Player1Id = dto.Team1Player1Id,
                Team1Player2Id = dto.Team1Player2Id,
                Team2Player1Id = dto.Team2Player1Id,
                Team2Player2Id = dto.Team2Player2Id,
                TournamentId = dto.TournamentId,
                Status = MatchStatus.Scheduled,
                Result = WinnerSide.None
            };

            await _unitOfWork.Matches.AddAsync(match);
            await _unitOfWork.SaveChangesAsync();

            return await MapToDtoAsync(match);
        }

        public async Task<MatchDto> UpdateMatchResultAsync(UpdateMatchResultDto dto)
        {
            var match = await _unitOfWork.Matches.GetByIdAsync(dto.MatchId);
            if (match == null)
                throw new Exception("Match not found");

            match.ScoreTeam1 = dto.ScoreTeam1;
            match.ScoreTeam2 = dto.ScoreTeam2;
            match.Result = dto.Result;
            match.Status = MatchStatus.Completed;

            if (match.IsRanked && dto.Result != WinnerSide.None && dto.Result != WinnerSide.Draw)
            {
                await ApplyEloAsync(match, dto.Result);
            }

            _unitOfWork.Matches.Update(match);

            if (match.TournamentId.HasValue)
            {
                await PromoteWinnerToNextRoundAsync(match);
            }

            await _unitOfWork.SaveChangesAsync();

            return await MapToDtoAsync(match);
        }

        public async Task<List<MatchDto>> GetMatchesAsync(int? tournamentId = null)
        {
            var matches = tournamentId.HasValue
                ? await _unitOfWork.Matches.FindAsync(m => m.TournamentId == tournamentId)
                : await _unitOfWork.Matches.GetAllAsync();

            var list = matches.OrderByDescending(m => m.Date).ToList();
            var result = new List<MatchDto>();
            foreach (var match in list)
            {
                result.Add(await MapToDtoAsync(match));
            }

            return result;
        }

        public async Task<MatchDto?> GetByIdAsync(int id)
        {
            var match = await _unitOfWork.Matches.GetByIdAsync(id);
            if (match == null)
                return null;

            return await MapToDtoAsync(match);
        }

        private async Task ApplyEloAsync(Match match, WinnerSide result)
        {
            var memberIds = new[] { match.Team1Player1Id, match.Team1Player2Id, match.Team2Player1Id, match.Team2Player2Id }
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var members = (await _unitOfWork.Members.GetAllAsync()).Where(m => memberIds.Contains(m.Id)).ToDictionary(m => m.Id);

            if (!members.TryGetValue(match.Team1Player1Id, out var t1p1) ||
                !members.TryGetValue(match.Team2Player1Id, out var t2p1))
                return;

            var t1p2 = match.Team1Player2Id.HasValue && members.TryGetValue(match.Team1Player2Id.Value, out var t1p2Member)
                ? t1p2Member
                : null;

            var t2p2 = match.Team2Player2Id.HasValue && members.TryGetValue(match.Team2Player2Id.Value, out var t2p2Member)
                ? t2p2Member
                : null;

            var team1Rating = _eloRatingService.CalculateTeamRating(t1p1.RankELO, t1p2?.RankELO);
            var team2Rating = _eloRatingService.CalculateTeamRating(t2p1.RankELO, t2p2?.RankELO);
            var team1Won = result == WinnerSide.Team1;

            var (newTeam1Rating, newTeam2Rating) = _eloRatingService.CalculateNewRatings(team1Rating, team2Rating, team1Won);
            var changeTeam1 = newTeam1Rating - team1Rating;
            var changeTeam2 = newTeam2Rating - team2Rating;

            t1p1.RankELO = Math.Round(t1p1.RankELO + changeTeam1, 2);
            if (t1p2 != null) t1p2.RankELO = Math.Round(t1p2.RankELO + changeTeam1, 2);

            t2p1.RankELO = Math.Round(t2p1.RankELO + changeTeam2, 2);
            if (t2p2 != null) t2p2.RankELO = Math.Round(t2p2.RankELO + changeTeam2, 2);

            match.EloChange = changeTeam1;

            _unitOfWork.Members.Update(t1p1);
            if (t1p2 != null) _unitOfWork.Members.Update(t1p2);
            _unitOfWork.Members.Update(t2p1);
            if (t2p2 != null) _unitOfWork.Members.Update(t2p2);

            await _cacheService.RemoveAsync("leaderboard:top:5");
            await _cacheService.AddToSortedSetAsync("leaderboard:elo", t1p1.FullName, t1p1.RankELO);
            if (t1p2 != null) await _cacheService.AddToSortedSetAsync("leaderboard:elo", t1p2.FullName, t1p2.RankELO);
            await _cacheService.AddToSortedSetAsync("leaderboard:elo", t2p1.FullName, t2p1.RankELO);
            if (t2p2 != null) await _cacheService.AddToSortedSetAsync("leaderboard:elo", t2p2.FullName, t2p2.RankELO);
        }

        private async Task PromoteWinnerToNextRoundAsync(Match match)
        {
            var tournamentMatch = (await _unitOfWork.TournamentMatches.FindAsync(tm => tm.MatchId == match.Id)).FirstOrDefault();
            if (tournamentMatch == null || !tournamentMatch.NextMatchId.HasValue)
                return;

            var nextTournamentMatch = await _unitOfWork.TournamentMatches.GetByIdAsync(tournamentMatch.NextMatchId.Value);
            if (nextTournamentMatch == null)
                return;

            var nextMatch = await _unitOfWork.Matches.GetByIdAsync(nextTournamentMatch.MatchId);
            if (nextMatch == null)
                return;

            var team1Winner = match.Result == WinnerSide.Team1;
            var winner1 = team1Winner ? match.Team1Player1Id : match.Team2Player1Id;
            var winner2 = team1Winner ? match.Team1Player2Id : match.Team2Player2Id;

            if (tournamentMatch.Position % 2 == 1)
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

        private async Task<MatchDto> MapToDtoAsync(Match match)
        {
            var members = (await _unitOfWork.Members.GetAllAsync()).ToDictionary(m => m.Id, m => m.FullName);
            var tournamentName = match.TournamentId.HasValue
                ? (await _unitOfWork.Tournaments.GetByIdAsync(match.TournamentId.Value))?.Name
                : null;

            return new MatchDto
            {
                Id = match.Id,
                Date = match.Date,
                IsRanked = match.IsRanked,
                MatchFormat = match.MatchFormat.ToString(),
                Team1Player1Id = match.Team1Player1Id,
                Team1Player1Name = members.TryGetValue(match.Team1Player1Id, out var t1p1) ? t1p1 : string.Empty,
                Team1Player2Id = match.Team1Player2Id,
                Team1Player2Name = match.Team1Player2Id.HasValue && members.TryGetValue(match.Team1Player2Id.Value, out var t1p2) ? t1p2 : null,
                Team2Player1Id = match.Team2Player1Id,
                Team2Player1Name = members.TryGetValue(match.Team2Player1Id, out var t2p1) ? t2p1 : string.Empty,
                Team2Player2Id = match.Team2Player2Id,
                Team2Player2Name = match.Team2Player2Id.HasValue && members.TryGetValue(match.Team2Player2Id.Value, out var t2p2) ? t2p2 : null,
                ScoreTeam1 = match.ScoreTeam1,
                ScoreTeam2 = match.ScoreTeam2,
                Result = match.Result.ToString(),
                EloChange = match.EloChange,
                TournamentId = match.TournamentId,
                TournamentName = tournamentName,
                Status = match.Status.ToString()
            };
        }
    }
}
