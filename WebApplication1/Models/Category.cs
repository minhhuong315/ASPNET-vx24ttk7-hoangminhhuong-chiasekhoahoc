using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public Category? Parent { get; set; }

        public ICollection<Category> Children { get; set; }
            = new List<Category>();

        public ICollection<Course> Courses { get; set; }
            = new List<Course>();
    }
}