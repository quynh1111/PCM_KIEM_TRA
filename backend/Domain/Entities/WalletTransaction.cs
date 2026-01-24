using System;
using PCM.Domain.Enums;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_WalletTransactions
    /// Tracks all wallet balance changes
    /// </summary>
    public class WalletTransaction
    {
        public int Id { get; set; }
        
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        
        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;
        
        public Guid CategoryId { get; set; }
        public TransactionCategory Category { get; set; } = default!;
        
        public WalletTransactionType Type { get; set; }
        
        /// <summary>
        /// Reference to related entity (Booking ID, Tournament ID, etc.)
        /// </summary>
        public string? ReferenceId { get; set; }
        
        public string? Description { get; set; }
        public WalletTransactionStatus Status { get; set; }
        
        /// <summary>
        /// SHA256 hash for transaction integrity
        /// </summary>
        public string? EncryptedSignature { get; set; }
        
        /// <summary>
        /// For deposit requests - proof of payment image URL
        /// </summary>
        public string? ProofImageUrl { get; set; }
    }
}
