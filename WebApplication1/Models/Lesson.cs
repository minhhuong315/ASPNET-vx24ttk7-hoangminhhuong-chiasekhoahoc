using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [StringLength(500)]
        public string? VideoUrl { get; set; }

        public int Duration { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsFree { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Module Module { get; set; } = null!;
        public ICollection<LessonProgress> LessonProgresses { get; set; }
            = new List<LessonProgress>();
    }
}