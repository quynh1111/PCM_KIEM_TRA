using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCM.Application.DTOs.Treasury;

namespace PCM.Application.Interfaces
{
    public interface ITreasuryService
    {
        Task<IReadOnlyList<TreasuryTransactionDto>> GetTransactionsAsync(CancellationToken ct = default);
        Task<TreasuryTransactionDto> CreateTransactionAsync(string createdByUserId, TreasuryTransactionCreateDto dto, CancellationToken ct = default);
        Task<TreasurySummaryDto> GetSummaryAsync(CancellationToken ct = default);
    }
}
