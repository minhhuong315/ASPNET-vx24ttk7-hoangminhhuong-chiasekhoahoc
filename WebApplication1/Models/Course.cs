using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(500)]
        public string? VideoPreviewUrl { get; set; }

        [StringLength(20)]
        public string Level { get; set; } = "Beginner";

        [StringLength(10)]
        public string Language { get; set; } = "vi";

        public int Duration { get; set; }

        public string InstructorId { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAt { get; set; }

        public bool IsPublished { get; set; } = false;

        public bool IsFeatured { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public int EnrollmentCount { get; set; } = 0;

        public ApplicationUser Instructor { get; set; } = null!;

        public Category Category { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
        public ICollection<Module> Modules { get; set; }
        = new List<Module>();
        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();
        public ICollection<Wishlist> Wishlists { get; set; }
            = new List<Wishlist>();
    }
}