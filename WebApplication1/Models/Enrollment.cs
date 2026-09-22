using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models
{
    public class Enrollment
    {
        public string UserId { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Progress { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        public string? CertificateUrl { get; set; }

        public ApplicationUser User { get; set; } = null!;

        public Course Course { get; set; } = null!;
    }
}