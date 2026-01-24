using System;

namespace PCM.Domain.Entities
{
    // Table name must be prefixed by last 3 digits of MSSV: 020_TransactionCategories
    public class TransactionCategory
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;  // Income / Expense
        public string Scope { get; set; } = default!; // Treasury / Wallet

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
