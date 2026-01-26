namespace PCM.Application.DTOs.Courts
{
    public class CourtUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool? IsActive { get; set; }
    }
}
