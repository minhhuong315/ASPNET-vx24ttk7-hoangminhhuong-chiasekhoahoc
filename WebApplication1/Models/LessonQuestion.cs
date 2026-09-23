using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatform.Models
{
    public class LessonQuestion
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;


        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Lesson Lesson { get; set; } = null!;


        [DeleteBehavior(DeleteBehavior.Restrict)]
        public ApplicationUser User { get; set; } = null!;


        public ICollection<LessonAnswer> Answers { get; set; }
            = new List<LessonAnswer>();
    }
}