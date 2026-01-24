using System;
using PCM.Domain.Enums;

namespace PCM.Application.DTOs.Treasury
{
    public class TransactionCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public TransactionType Type { get; set; }
        public TransactionScope Scope { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
