using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models
{
    public class Notification
    {
        public int Id { get; set; }


        // =========================
        // NGƯỜI NHẬN THÔNG BÁO
        // =========================

        [Required]
        public string UserId { get; set; } = string.Empty;


        // =========================
        // NỘI DUNG
        // =========================

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;


        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;


        /*
         * Ví dụ:
         *
         * NewQuestion
         * QuestionAnswered
         * CourseUpdated
         * NewLesson
         * System
         */
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "System";


        // =========================
        // LINK KHI BẤM THÔNG BÁO
        // =========================

        [StringLength(500)]
        public string? RelatedUrl { get; set; }


        // =========================
        // TRẠNG THÁI ĐỌC
        // =========================

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }


        // =========================
        // THỜI GIAN
        // =========================

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        // =========================
        // NAVIGATION
        // =========================

        public ApplicationUser User { get; set; } = null!;
    }
}