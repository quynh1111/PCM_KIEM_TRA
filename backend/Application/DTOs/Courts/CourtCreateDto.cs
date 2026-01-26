namespace PCM.Application.DTOs.Courts
{
    public class CourtCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
