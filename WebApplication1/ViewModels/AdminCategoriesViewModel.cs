using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    public class AdminCategoriesPageViewModel
    {
        public string? Search { get; set; }

        public string? Status { get; set; }


        public int TotalCategories { get; set; }

        public int ActiveCategories { get; set; }

        public int InactiveCategories { get; set; }

        public int TotalCourses { get; set; }


        public List<AdminCategoryItemViewModel>
            Categories
        { get; set; }
                = new List<AdminCategoryItemViewModel>();
    }


    public class AdminCategoryItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string? Slug { get; set; }
            = null;

        public string? Description { get; set; }

        public string? Icon { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public int CourseCount { get; set; }
    }


    public class AdminCategoryFormViewModel
    {
        public int? Id { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(
            100,
            ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }
            = string.Empty;


        [StringLength(
            100,
            ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; }
            = null;


        [StringLength(
            1000,
            ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }


        [StringLength(
            50,
            ErrorMessage = "Icon không được vượt quá 50 ký tự.")]
        [Display(Name = "Icon")]
        public string? Icon { get; set; }


        [Range(
            1,
            9999,
            ErrorMessage = "Thứ tự hiển thị phải từ 1 trở lên.")]
        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }
            = 1;


        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; }
            = true;
    }
}
