using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.Matches;

namespace PCM.Application.Interfaces
{
    public interface IMatchService
    {
        Task<MatchDto> CreateMatchAsync(CreateMatchDto dto);
        
        /// <summary>
        /// Update match result and calculate ELO rating changes
        /// Sends SignalR notification for live score
        /// </summary>
        Task<MatchDto> UpdateMatchResultAsync(UpdateMatchResultDto dto);
        
        Task<List<MatchDto>> GetMatchesAsync(int? tournamentId = null);
        Task<MatchDto?> GetByIdAsync(int id);
    }
}
