using System;

namespace PCM.Domain.Entities
{
    // Table name must be prefixed by last 3 digits of MSSV: 020_TreasuryTransactions
    public class TreasuryTransaction
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// Quy ước:
        /// - Thu: Amount dương
        /// - Chi: Amount âm
        /// => Balance = SUM(Amount)
        /// </summary>
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public Guid CategoryId { get; set; }
        public TransactionCategory Category { get; set; } = default!;

        public string CreatedByMemberId { get; set; } = default!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
