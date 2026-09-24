using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    // =========================================
    // DANH SÁCH KHÓA HỌC
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
        public string Title { get; set; }
            = string.Empty;


        [StringLength(
            500,
            ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự.")]
        public string? ShortDescription { get; set; }


        public string? Description { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }


        [Required(
            ErrorMessage = "Vui lòng chọn cấp độ.")]
        public string Level { get; set; }
            = "Beginner";


        [Range(
            typeof(decimal),
            "0",
            "999999999",
            ErrorMessage = "Giá khóa học không hợp lệ.")]
        public decimal Price { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "999999999",
            ErrorMessage = "Giá khuyến mãi không hợp lệ.")]
        public decimal? DiscountPrice { get; set; }


        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }


        [StringLength(500)]
        public string? VideoPreviewUrl { get; set; }


        public List<InstructorCourseCategoryOptionViewModel>
            Categories
        { get; set; }
                = new List<InstructorCourseCategoryOptionViewModel>();
    }


    // =========================================
    // SỬA KHÓA HỌC
    // =========================================

    public class InstructorCourseEditViewModel
    {
        public int Id { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập tên khóa học.")]
        [StringLength(
            200,
            ErrorMessage = "Tên khóa học không được vượt quá 200 ký tự.")]
        public string Title { get; set; }
            = string.Empty;


        [StringLength(
            500,
            ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự.")]
        public string? ShortDescription { get; set; }


        public string? Description { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }


        [Required(
            ErrorMessage = "Vui lòng chọn cấp độ.")]
        public string Level { get; set; }
            = "Beginner";


        [Range(
            typeof(decimal),
            "0",
            "999999999",
            ErrorMessage = "Giá khóa học không hợp lệ.")]
        public decimal Price { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "999999999",
            ErrorMessage = "Giá khuyến mãi không hợp lệ.")]
        public decimal? DiscountPrice { get; set; }


        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }


        [StringLength(500)]
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


    // =========================================
    // TẠO MODULE
    // =========================================

    public class InstructorModuleCreateViewModel
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập tên module.")]
        [StringLength(
            200,
            ErrorMessage = "Tên module không được vượt quá 200 ký tự.")]
        public string Title { get; set; }
            = string.Empty;


        [StringLength(
            1000,
            ErrorMessage = "Mô tả module không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }
    }


    // =========================================
    // SỬA MODULE
    // =========================================

    public class InstructorModuleEditViewModel
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập tên module.")]
        [StringLength(
            200,
            ErrorMessage = "Tên module không được vượt quá 200 ký tự.")]
        public string Title { get; set; }
            = string.Empty;


        [StringLength(
            1000,
            ErrorMessage = "Mô tả module không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Thứ tự module phải lớn hơn 0.")]
        public int DisplayOrder { get; set; }
    }


    // =========================================
    // TẠO BÀI HỌC
    // =========================================

    public class InstructorLessonCreateViewModel
    {
        public int ModuleId { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }
            = string.Empty;

        public string ModuleTitle { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập tên bài học.")]
        [StringLength(
            200,
            ErrorMessage = "Tên bài học không được vượt quá 200 ký tự.")]
        public string Title { get; set; }
            = string.Empty;


        public string? Content { get; set; }


        [StringLength(
            500,
            ErrorMessage = "Đường dẫn video không được vượt quá 500 ký tự.")]
        public string? VideoUrl { get; set; }


        [Range(
            0,
            1440,
            ErrorMessage = "Thời lượng phải từ 0 đến 1440 phút.")]
        public int Duration { get; set; }


        public bool IsFree { get; set; }
    }


    // =========================================
    // SỬA BÀI HỌC
    // =========================================

    public class InstructorLessonEditViewModel
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }
            = string.Empty;

        public string ModuleTitle { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập tên bài học.")]
        [StringLength(
            200,
            ErrorMessage = "Tên bài học không được vượt quá 200 ký tự.")]
        public string Title { get; set; }
            = string.Empty;


        public string? Content { get; set; }


        [StringLength(
            500,
            ErrorMessage = "Đường dẫn video không được vượt quá 500 ký tự.")]
        public string? VideoUrl { get; set; }


        [Range(
            0,
            1440,
            ErrorMessage = "Thời lượng phải từ 0 đến 1440 phút.")]
        public int Duration { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Thứ tự bài học phải lớn hơn 0.")]
        public int DisplayOrder { get; set; }


        public bool IsFree { get; set; }
    }
}