namespace PCM.Application.DTOs.News
{
    public class CreateNewsDto
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool IsPinned { get; set; }
    }
}
