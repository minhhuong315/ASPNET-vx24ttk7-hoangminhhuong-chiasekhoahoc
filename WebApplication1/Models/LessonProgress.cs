namespace OnlineLearningPlatform.Models
{
    public class LessonProgress
    {
        public string UserId { get; set; } = string.Empty;

        public int LessonId { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int WatchedDuration { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;

        public Lesson Lesson { get; set; } = null!;
    }
}