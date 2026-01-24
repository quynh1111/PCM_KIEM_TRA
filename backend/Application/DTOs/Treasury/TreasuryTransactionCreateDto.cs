using System;

namespace PCM.Application.DTOs.Treasury
{
    public class TreasuryTransactionCreateDto
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
    }
}
