using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Avatar { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }
        public ICollection<Course> Courses { get; set; }
    = new List<Course>();
        public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
        public ICollection<LessonProgress> LessonProgresses { get; set; }
            = new List<LessonProgress>();
        public ICollection<Review> Reviews { get; set; }
       = new List<Review>();
        public ICollection<Wishlist> Wishlists { get; set; }
            = new List<Wishlist>();
    }
}