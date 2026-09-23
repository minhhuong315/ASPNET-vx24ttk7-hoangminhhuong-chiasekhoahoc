namespace OnlineLearningPlatform.ViewModels
{
    public class HomeIndexViewModel
    {
        // =========================================
        // KHÓA HỌC CỦA STUDENT
        // =========================================

        public List<HomeCourseViewModel> StudentCourses { get; set; }
            = new List<HomeCourseViewModel>();


        // =========================================
        // THỐNG KÊ STUDENT
        // =========================================

        public int RegisteredCourseCount { get; set; }

        public int CompletedLessonCount { get; set; }

        public decimal AverageProgress { get; set; }
    }


    public class HomeCourseViewModel
    {
        public int CourseId { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Slug { get; set; }
            = string.Empty;

        public decimal Progress { get; set; }

        public int CompletedLessonCount { get; set; }

        public int TotalLessonCount { get; set; }

        public int? ContinueLessonId { get; set; }
    }
}