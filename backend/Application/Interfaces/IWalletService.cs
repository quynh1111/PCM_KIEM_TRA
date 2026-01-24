using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.Wallet;

namespace PCM.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletTransactionDto> CreateDepositRequestAsync(string userId, WalletDepositRequestDto dto);
        Task<WalletTransactionDto> ApproveDepositAsync(int transactionId, bool approved, string? note = null);
        Task<List<WalletTransactionDto>> GetTransactionHistoryAsync(string userId);
        Task<decimal> GetBalanceAsync(string userId);
        
        /// <summary>
        /// CRITICAL: Thread-safe wallet deduction with transaction support
        /// </summary>
        Task<bool> DeductBalanceAsync(int memberId, decimal amount, string referenceId, string description);
        
        /// <summary>
        /// Add balance (for refunds or prizes)
        /// </summary>
        Task<bool> AddBalanceAsync(int memberId, decimal amount, string referenceId, string description);
    }
}
