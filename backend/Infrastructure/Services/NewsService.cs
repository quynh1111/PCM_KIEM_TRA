using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PCM.Application.DTOs.News;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class NewsService : INewsService
    {
        private const string PinnedCacheKey = "news:pinned";
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public NewsService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<List<NewsDto>> GetAllAsync(bool onlyPinned = false)
        {
            if (onlyPinned)
            {
                var cached = await _cacheService.GetAsync<List<NewsDto>>(PinnedCacheKey);
                if (cached != null && cached.Any())
                    return cached;
            }

            var news = await _unitOfWork.News.GetAllAsync();
            var filtered = onlyPinned ? news.Where(n => n.IsPinned) : news;
            var result = filtered
                .OrderByDescending(n => n.CreatedDate)
                .Select(MapToDto)
                .ToList();

            if (onlyPinned)
            {
                await _cacheService.SetAsync(PinnedCacheKey, result, TimeSpan.FromMinutes(10));
            }

            return result;
        }

        public async Task<NewsDto?> GetByIdAsync(int id)
        {
            var news = await _unitOfWork.News.GetByIdAsync(id);
            return news == null ? null : MapToDto(news);
        }

        public async Task<NewsDto> CreateAsync(CreateNewsDto dto, string createdBy)
        {
            var entity = new News
            {
                Title = dto.Title,
                Content = dto.Content,
                IsPinned = dto.IsPinned,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.News.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            if (dto.IsPinned)
                await _cacheService.RemoveAsync(PinnedCacheKey);

            return MapToDto(entity);
        }

        public async Task<NewsDto?> UpdateAsync(int id, CreateNewsDto dto)
        {
            var entity = await _unitOfWork.News.GetByIdAsync(id);
            if (entity == null)
                return null;

            entity.Title = dto.Title;
            entity.Content = dto.Content;
            entity.IsPinned = dto.IsPinned;

            _unitOfWork.News.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(PinnedCacheKey);

            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.News.GetByIdAsync(id);
            if (entity == null)
                return false;

            _unitOfWork.News.Remove(entity);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveAsync(PinnedCacheKey);
            return true;
        }

        private static NewsDto MapToDto(News entity)
        {
            return new NewsDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Content = entity.Content,
                IsPinned = entity.IsPinned,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate
            };
        }
    }
}
