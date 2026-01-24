using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCM.Application.DTOs.Treasury;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class TransactionCategoryService : ITransactionCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<TransactionCategoryDto>> GetAsync(TransactionScope? scope = null, CancellationToken ct = default)
        {
            var categories = await _unitOfWork.TransactionCategories.GetAllAsync();
            var filtered = scope.HasValue
                ? categories.Where(c => string.Equals(c.Scope, scope.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                : categories;

            return filtered.Select(MapToDto).ToList();
        }

        public async Task<TransactionCategoryDto> CreateAsync(TransactionCategoryCreateDto dto, CancellationToken ct = default)
        {
            var entity = new TransactionCategory
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Type = dto.Type.ToString(),
                Scope = dto.Scope.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TransactionCategories.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<TransactionCategoryDto> UpdateAsync(Guid id, TransactionCategoryCreateDto dto, CancellationToken ct = default)
        {
            var entity = await _unitOfWork.TransactionCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
                throw new Exception("Category not found");

            entity.Name = dto.Name;
            entity.Type = dto.Type.ToString();
            entity.Scope = dto.Scope.ToString();

            _unitOfWork.TransactionCategories.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _unitOfWork.TransactionCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
                return;

            _unitOfWork.TransactionCategories.Remove(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        private static TransactionCategoryDto MapToDto(TransactionCategory entity)
        {
            Enum.TryParse(entity.Type, true, out TransactionType type);
            Enum.TryParse(entity.Scope, true, out TransactionScope scope);

            return new TransactionCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = type,
                Scope = scope,
                CreatedDate = entity.CreatedDate
            };
        }
    }
}
