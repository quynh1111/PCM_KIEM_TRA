using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCM.Application.DTOs.Treasury;
using PCM.Domain.Enums;

namespace PCM.Application.Interfaces
{
    public interface ITransactionCategoryService
    {
        Task<IReadOnlyList<TransactionCategoryDto>> GetAsync(TransactionScope? scope = null, CancellationToken ct = default);
        Task<TransactionCategoryDto> CreateAsync(TransactionCategoryCreateDto dto, CancellationToken ct = default);
        Task<TransactionCategoryDto> UpdateAsync(Guid id, TransactionCategoryCreateDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
