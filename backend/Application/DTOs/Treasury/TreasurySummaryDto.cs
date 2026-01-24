namespace PCM.Application.DTOs.Treasury
{
    public class TreasurySummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; } // âm hoặc dương tuỳ cách tính, ở đây trả dương cho dễ hiển thị
        public decimal Balance { get; set; }
    }
}
