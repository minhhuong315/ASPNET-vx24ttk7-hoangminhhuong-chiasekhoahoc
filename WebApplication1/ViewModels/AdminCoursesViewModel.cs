namespace OnlineLearningPlatform.ViewModels
{
    public class AdminCoursesPageViewModel
    {
        public string? Search { get; set; }

        public string? Status { get; set; }

        public int? CategoryId { get; set; }


        public int TotalCourses { get; set; }

        public int PublishedCourses { get; set; }

        public int DraftCourses { get; set; }

        public int TotalEnrollments { get; set; }


        public List<AdminCourseItemViewModel>
            Courses
        { get; set; }
                = new List<AdminCourseItemViewModel>();


        public List<AdminCourseCategoryOptionViewModel>
            Categories
        { get; set; }
                = new List<AdminCourseCategoryOptionViewModel>();
    }


    public class AdminCourseItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Slug { get; set; }
            = string.Empty;

        public string CategoryName { get; set; }
            = string.Empty;

        public string InstructorName { get; set; }
            = string.Empty;

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; }

        public int EnrollmentCount { get; set; }

        public int ModuleCount { get; set; }

        public int LessonCount { get; set; }

        public int ReviewCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }


    public class AdminCourseCategoryOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;
    }
}
