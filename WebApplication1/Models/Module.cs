using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models
{
    public class Module
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = null!;

        public ICollection<Lesson> Lessons { get; set; }
            = new List<Lesson>();
    }
}