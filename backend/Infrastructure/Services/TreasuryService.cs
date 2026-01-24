using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCM.Application.DTOs.Treasury;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class TreasuryService : ITreasuryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TreasuryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<TreasuryTransactionDto>> GetTransactionsAsync(CancellationToken ct = default)
        {
            var transactions = await _unitOfWork.TreasuryTransactions.GetAllAsync();
            var categories = await _unitOfWork.TransactionCategories.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

            return transactions
                .OrderByDescending(t => t.Date)
                .Select(t => MapToDto(t, categoryMap))
                .ToList();
        }

        public async Task<TreasuryTransactionDto> CreateTransactionAsync(string createdByUserId, TreasuryTransactionCreateDto dto, CancellationToken ct = default)
        {
            var category = await _unitOfWork.TransactionCategories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
            if (category == null)
                throw new Exception("Category not found");

            var entity = new TreasuryTransaction
            {
                Id = Guid.NewGuid(),
                Date = dto.Date,
                Amount = dto.Amount,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                CreatedByMemberId = createdByUserId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TreasuryTransactions.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity, category.Name);
        }

        public async Task<TreasurySummaryDto> GetSummaryAsync(CancellationToken ct = default)
        {
            var transactions = await _unitOfWork.TreasuryTransactions.GetAllAsync();

            var totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
            var balance = transactions.Sum(t => t.Amount);

            return new TreasurySummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance
            };
        }

        private static TreasuryTransactionDto MapToDto(TreasuryTransaction transaction, IDictionary<Guid, string> categoryMap)
        {
            categoryMap.TryGetValue(transaction.CategoryId, out var categoryName);

            return new TreasuryTransactionDto
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Description = transaction.Description,
                CategoryId = transaction.CategoryId,
                CategoryName = categoryName ?? string.Empty,
                CreatedByMemberId = transaction.CreatedByMemberId,
                CreatedDate = transaction.CreatedDate
            };
        }

        private static TreasuryTransactionDto MapToDto(TreasuryTransaction transaction, string categoryName)
        {
            return new TreasuryTransactionDto
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Description = transaction.Description,
                CategoryId = transaction.CategoryId,
                CategoryName = categoryName,
                CreatedByMemberId = transaction.CreatedByMemberId,
                CreatedDate = transaction.CreatedDate
            };
        }
    }
}
