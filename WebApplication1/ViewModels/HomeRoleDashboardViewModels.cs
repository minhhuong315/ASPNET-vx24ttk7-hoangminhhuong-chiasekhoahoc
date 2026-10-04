namespace OnlineLearningPlatform.ViewModels
{
    public class StudentHomeDashboardViewModel
    {
        public int RegisteredCourseCount { get; set; }

        public int InProgressCourseCount { get; set; }

        public int CompletedCourseCount { get; set; }

        public int WishlistCount { get; set; }

        public int OpenQuestionCount { get; set; }

        public decimal AverageProgress { get; set; }

        public List<StudentHomeCourseItemViewModel>
            RecentCourses { get; set; } = new();

        public List<StudentHomeExploreCourseViewModel>
            ExploreCourses { get; set; } = new();
    }


    public class StudentHomeCourseItemViewModel
    {
        public int CourseId { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public string Slug { get; set; } =
            string.Empty;

        public string CategoryName { get; set; } =
            string.Empty;

        public string CategoryIcon { get; set; } =
            "IT";

        public string? ThumbnailUrl { get; set; }

        public decimal Progress { get; set; }

        public DateTime EnrolledAt { get; set; }

        public DateTime? LastAccessedAt { get; set; }
    }


    public class StudentHomeExploreCourseViewModel
    {
        public int CourseId { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public string Slug { get; set; } =
            string.Empty;

        public string CategoryName { get; set; } =
            string.Empty;

        public string CategoryIcon { get; set; } =
            "IT";

        public string? ThumbnailUrl { get; set; }

        public string Level { get; set; } =
            string.Empty;

        public int EnrollmentCount { get; set; }
    }


    public class InstructorHomeDashboardViewModel
    {
        public int TotalCourses { get; set; }

        public int PublishedCourses { get; set; }

        public int DraftCourses { get; set; }

        public int UniqueStudents { get; set; }

        public int PendingQuestions { get; set; }

        public double AverageRating { get; set; }

        public int TotalLessons { get; set; }

        public List<InstructorHomeCourseItemViewModel>
            RecentCourses { get; set; } = new();
    }


    public class InstructorHomeCourseItemViewModel
    {
        public int CourseId { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public string CategoryName { get; set; } =
            string.Empty;

        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; }

        public int LessonCount { get; set; }

        public int EnrollmentCount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }


    public class AdminHomeDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int StudentCount { get; set; }

        public int InstructorCount { get; set; }

        public int AdminCount { get; set; }

        public int TotalCourses { get; set; }

        public int PublishedCourses { get; set; }

        public int TotalEnrollments { get; set; }

        public int DraftCourses { get; set; }

        public int ActiveCategories { get; set; }

        public int PendingQuestions { get; set; }

        public List<AdminEnrollmentTrendItemViewModel>
            EnrollmentTrend { get; set; } = new();

        public List<AdminHomeCourseItemViewModel>
            RecentCourses { get; set; } = new();
    }


    public class AdminEnrollmentTrendItemViewModel
    {
        public DateTime Date { get; set; }

        public string Label { get; set; } =
            string.Empty;

        public int Count { get; set; }
    }


    public class AdminHomeCourseItemViewModel
    {
        public int CourseId { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public string InstructorName { get; set; } =
            string.Empty;

        public string CategoryName { get; set; } =
            string.Empty;

        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; }

        public int EnrollmentCount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
