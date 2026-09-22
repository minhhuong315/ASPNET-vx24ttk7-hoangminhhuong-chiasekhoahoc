namespace OnlineLearningPlatform.Models
{
    public class Wishlist
    {
        public string UserId { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;

        public Course Course { get; set; } = null!;
    }
}