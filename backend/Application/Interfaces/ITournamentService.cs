using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.Tournaments;

namespace PCM.Application.Interfaces
{
    public interface ITournamentService
    {
        Task<List<TournamentDto>> GetAllTournamentsAsync();
        Task<TournamentDto?> GetByIdAsync(int id);
        Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto, string userId);
        Task<bool> JoinTournamentAsync(int tournamentId, string userId, string? teamName = null);
        Task<BracketDto?> GetBracketAsync(int tournamentId);
        Task<bool> GenerateBracketAsync(int tournamentId);
    }
}
