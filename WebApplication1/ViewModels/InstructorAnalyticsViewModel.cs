namespace OnlineLearningPlatform.ViewModels
{
    public class InstructorAnalyticsViewModel
    {
        public int TotalCourses { get; set; }

        public int PublishedCourses { get; set; }

        public int DraftCourses { get; set; }

        public int TotalEnrollments { get; set; }

        public int UniqueStudents { get; set; }

        public int CompletedEnrollments { get; set; }

        public decimal AverageProgress { get; set; }

        public decimal CompletionRate { get; set; }

        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }


        public List<InstructorCourseAnalyticsItemViewModel>
            Courses
        { get; set; }
                = new List<InstructorCourseAnalyticsItemViewModel>();
    }


    public class InstructorCourseAnalyticsItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Slug { get; set; }
            = string.Empty;

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int TotalLessons { get; set; }

        public int EnrollmentCount { get; set; }

        public int CompletedStudents { get; set; }

        public decimal AverageProgress { get; set; }

        public int ReviewCount { get; set; }

        public double AverageRating { get; set; }
    }
}
