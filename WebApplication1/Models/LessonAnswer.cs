using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatform.Models
{
    public class LessonAnswer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        [DeleteBehavior(DeleteBehavior.Cascade)]
        public LessonQuestion Question { get; set; } = null!;


        [DeleteBehavior(DeleteBehavior.Restrict)]
        public ApplicationUser User { get; set; } = null!;
    }
}