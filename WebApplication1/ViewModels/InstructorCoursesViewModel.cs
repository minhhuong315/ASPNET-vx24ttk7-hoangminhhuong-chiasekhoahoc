namespace OnlineLearningPlatform.ViewModels
{
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
}