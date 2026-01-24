using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.News;

namespace PCM.Application.Interfaces
{
    public interface INewsService
    {
        Task<List<NewsDto>> GetAllAsync(bool onlyPinned = false);
        Task<NewsDto?> GetByIdAsync(int id);
        Task<NewsDto> CreateAsync(CreateNewsDto dto, string createdBy);
        Task<NewsDto?> UpdateAsync(int id, CreateNewsDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
