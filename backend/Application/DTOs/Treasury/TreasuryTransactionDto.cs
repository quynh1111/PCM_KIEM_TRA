using System;

namespace PCM.Application.DTOs.Treasury
{
    public class TreasuryTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public string CreatedByMemberId { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
    }
}
