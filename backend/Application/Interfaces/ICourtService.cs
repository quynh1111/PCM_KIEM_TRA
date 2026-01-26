using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.Courts;

namespace PCM.Application.Interfaces
{
    public interface ICourtService
    {
        Task<List<CourtDto>> GetAllAsync(bool includeInactive = false);
        Task<CourtDto?> GetByIdAsync(int id);
        Task<CourtDto> CreateAsync(CourtCreateDto dto);
        Task<CourtDto?> UpdateAsync(int id, CourtUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
