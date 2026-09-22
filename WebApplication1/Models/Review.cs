using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models
{
    public class Review
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int CourseId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = true;

        public ApplicationUser User { get; set; } = null!;

        public Course Course { get; set; } = null!;
    }
}