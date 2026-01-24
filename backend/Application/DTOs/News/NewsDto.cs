namespace PCM.Application.DTOs.News
{
    public class NewsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool IsPinned { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = default!;
    }
}
