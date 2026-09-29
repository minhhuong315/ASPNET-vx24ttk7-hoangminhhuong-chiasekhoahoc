namespace OnlineLearningPlatform.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalStudents { get; set; }

        public int TotalInstructors { get; set; }

        public int TotalCourses { get; set; }

        public int PublishedCourses { get; set; }

        public int DraftCourses { get; set; }

        public int TotalEnrollments { get; set; }

        public int TotalReviews { get; set; }


        public List<AdminRecentCourseViewModel>
            RecentCourses
        { get; set; }
                = new List<AdminRecentCourseViewModel>();
    }


    public class AdminRecentCourseViewModel
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

        public bool IsPublished { get; set; }

        public int EnrollmentCount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
