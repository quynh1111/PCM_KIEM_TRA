using System;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_News
    /// </summary>
    public class News
    {
        public int Id { get; set; }
        
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        
        /// <summary>
        /// Pinned news will be cached by Redis
        /// </summary>
        public bool IsPinned { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = default!;
    }
}
