using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    // =========================================
    // DANH SÁCH KHÓA HỌC GIẢNG VIÊN
    // =========================================

    public class InstructorCoursesViewModel
    {
        public List<InstructorCourseItemViewModel> Courses { get; set; }
            = new List<InstructorCourseItemViewModel>();

        public int TotalCourses { get; set; }

        public int PublishedCourses { get; set; }

        public int DraftCourses { get; set; }

        public int TotalLessons { get; set; }

        public int TotalEnrollments { get; set; }
    }


    public class InstructorCourseItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Slug { get; set; }
            = string.Empty;

        public string CategoryName { get; set; }
            = string.Empty;

        public string CategoryIcon { get; set; }
            = "IT";

        public string Level { get; set; }
            = "Beginner";

        public decimal Price { get; set; }

        public bool IsPublished { get; set; }

        public int ModuleCount { get; set; }

        public int LessonCount { get; set; }

        public int EnrollmentCount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }


    // =========================================
    // TẠO KHÓA HỌC
    // =========================================

    public class InstructorCourseCreateViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập tên khóa học.")]
        [StringLength(
            200,
            ErrorMessage = "Tên khóa học không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên khóa học")]
        public string Title { get; set; }
            = string.Empty;


        [StringLength(
            500,
            ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự.")]
        [Display(Name = "Mô tả ngắn")]
        public string? ShortDescription { get; set; }


        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }


        [Required(
            ErrorMessage = "Vui lòng chọn cấp độ.")]
        [Display(Name = "Cấp độ")]
        public string Level { get; set; }
            = "Beginner";


        [Range(
            typeof(decimal),
            "0",
            "999999999",
            ErrorMessage = "Giá khóa học không hợp lệ.")]
        [Display(Name = "Giá khóa học")]
        public decimal Price { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "999999999",
            ErrorMessage = "Giá khuyến mãi không hợp lệ.")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? DiscountPrice { get; set; }


        [StringLength(
            500,
            ErrorMessage = "Đường dẫn ảnh không được vượt quá 500 ký tự.")]
        [Display(Name = "Ảnh đại diện")]
        public string? ThumbnailUrl { get; set; }


        [StringLength(
            500,
            ErrorMessage = "Đường dẫn video không được vượt quá 500 ký tự.")]
        [Display(Name = "Video giới thiệu")]
        public string? VideoPreviewUrl { get; set; }


        public List<InstructorCourseCategoryOptionViewModel>
            Categories
        { get; set; }
                = new List<InstructorCourseCategoryOptionViewModel>();
    }


    public class InstructorCourseCategoryOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;
    }
}