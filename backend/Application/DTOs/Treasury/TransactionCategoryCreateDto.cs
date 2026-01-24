using PCM.Domain.Enums;

namespace PCM.Application.DTOs.Treasury
{
    public class TransactionCategoryCreateDto
    {
        public string Name { get; set; } = default!;
        public TransactionType Type { get; set; }
        public TransactionScope Scope { get; set; } = TransactionScope.Treasury;
    }
}
