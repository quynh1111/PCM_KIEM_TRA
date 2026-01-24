using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PCM.Application.DTOs.Wallet;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WalletTransactionDto> CreateDepositRequestAsync(string userId, WalletDepositRequestDto dto)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Member not found");

            var category = await GetOrCreateCategoryAsync("Deposit", TransactionType.Income, TransactionScope.Wallet);

            var transaction = new WalletTransaction
            {
                MemberId = member.Id,
                Date = DateTime.UtcNow,
                Amount = dto.Amount,
                CategoryId = category.Id,
                Type = WalletTransactionType.Deposit,
                ReferenceId = $"DEP-{Guid.NewGuid():N}",
                Description = dto.Description,
                Status = WalletTransactionStatus.Pending,
                ProofImageUrl = dto.ProofImageUrl
            };
            transaction.EncryptedSignature = ComputeSignature(transaction);

            await _unitOfWork.WalletTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(transaction, member, category);
        }

        public async Task<WalletTransactionDto> ApproveDepositAsync(int transactionId, bool approved, string? note = null)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var transaction = await _unitOfWork.WalletTransactions.GetByIdAsync(transactionId);
                if (transaction == null)
                    throw new Exception("Transaction not found");

                if (transaction.Status != WalletTransactionStatus.Pending)
                    throw new Exception("Transaction already processed");

                var member = await _unitOfWork.Members.GetByIdAsync(transaction.MemberId);
                if (member == null)
                    throw new Exception("Member not found");

                var category = await _unitOfWork.TransactionCategories.FirstOrDefaultAsync(c => c.Id == transaction.CategoryId)
                               ?? await GetOrCreateCategoryAsync("Deposit", TransactionType.Income, TransactionScope.Wallet);

                if (approved)
                {
                    member.WalletBalance += transaction.Amount;
                    transaction.Status = WalletTransactionStatus.Success;
                }
                else
                {
                    transaction.Status = WalletTransactionStatus.Failed;
                }

                if (!string.IsNullOrWhiteSpace(note))
                {
                    transaction.Description = string.IsNullOrWhiteSpace(transaction.Description)
                        ? note
                        : $"{transaction.Description} - {note}";
                }

                transaction.EncryptedSignature = ComputeSignature(transaction);
                _unitOfWork.WalletTransactions.Update(transaction);
                _unitOfWork.Members.Update(member);

                await _unitOfWork.CommitTransactionAsync();

                return MapToDto(transaction, member, category);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<WalletTransactionDto>> GetTransactionHistoryAsync(string userId)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Member not found");

            var transactions = await _unitOfWork.WalletTransactions.FindAsync(t => t.MemberId == member.Id);
            var categories = await _unitOfWork.TransactionCategories.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c);

            return transactions
                .OrderByDescending(t => t.Date)
                .Select(t =>
                {
                    categoryMap.TryGetValue(t.CategoryId, out var cat);
                    return MapToDto(t, member, cat);
                })
                .ToList();
        }

        public async Task<decimal> GetBalanceAsync(string userId)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Member not found");

            return member.WalletBalance;
        }

        public async Task<bool> DeductBalanceAsync(int memberId, decimal amount, string referenceId, string description)
        {
            if (amount <= 0)
                return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var member = await _unitOfWork.Members.GetByIdAsync(memberId);
                if (member == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                if (member.WalletBalance < amount)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var category = await GetOrCreateCategoryAsync("PayBooking", TransactionType.Expense, TransactionScope.Wallet);

                member.WalletBalance -= amount;

                var transaction = new WalletTransaction
                {
                    MemberId = member.Id,
                    Date = DateTime.UtcNow,
                    Amount = -amount,
                    CategoryId = category.Id,
                    Type = WalletTransactionType.PayBooking,
                    ReferenceId = referenceId,
                    Description = description,
                    Status = WalletTransactionStatus.Success
                };
                transaction.EncryptedSignature = ComputeSignature(transaction);

                await _unitOfWork.WalletTransactions.AddAsync(transaction);
                _unitOfWork.Members.Update(member);

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> AddBalanceAsync(int memberId, decimal amount, string referenceId, string description)
        {
            if (amount <= 0)
                return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var member = await _unitOfWork.Members.GetByIdAsync(memberId);
                if (member == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var category = await GetOrCreateCategoryAsync("Refund", TransactionType.Income, TransactionScope.Wallet);

                member.WalletBalance += amount;

                var transaction = new WalletTransaction
                {
                    MemberId = member.Id,
                    Date = DateTime.UtcNow,
                    Amount = amount,
                    CategoryId = category.Id,
                    Type = WalletTransactionType.Refund,
                    ReferenceId = referenceId,
                    Description = description,
                    Status = WalletTransactionStatus.Success
                };
                transaction.EncryptedSignature = ComputeSignature(transaction);

                await _unitOfWork.WalletTransactions.AddAsync(transaction);
                _unitOfWork.Members.Update(member);

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<TransactionCategory> GetOrCreateCategoryAsync(string name, TransactionType type, TransactionScope scope)
        {
            var category = await _unitOfWork.TransactionCategories
                .FirstOrDefaultAsync(c =>
                    c.Name == name &&
                    c.Type == type.ToString() &&
                    c.Scope == scope.ToString());

            if (category != null)
                return category;

            category = new TransactionCategory
            {
                Id = Guid.NewGuid(),
                Name = name,
                Type = type.ToString(),
                Scope = scope.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TransactionCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        private static WalletTransactionDto MapToDto(WalletTransaction transaction, Member member, TransactionCategory? category)
        {
            return new WalletTransactionDto
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                MemberId = member.Id,
                MemberName = member.FullName,
                CategoryId = transaction.CategoryId,
                CategoryName = category?.Name ?? string.Empty,
                Type = transaction.Type.ToString(),
                ReferenceId = transaction.ReferenceId,
                Description = transaction.Description,
                Status = transaction.Status.ToString(),
                ProofImageUrl = transaction.ProofImageUrl
            };
        }

        private static string ComputeSignature(WalletTransaction transaction)
        {
            var input = $"{transaction.MemberId}|{transaction.Amount}|{transaction.Date:O}|{transaction.ReferenceId}|{transaction.Status}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
