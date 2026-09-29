namespace OnlineLearningPlatform.ViewModels
{
    public class InstructorStudentsPageViewModel
    {
        public List<InstructorStudentCourseOptionViewModel>
            Courses
        { get; set; }
                = new List<InstructorStudentCourseOptionViewModel>();


        public int? SelectedCourseId { get; set; }

        public string SelectedCourseTitle { get; set; }
            = string.Empty;

        public bool SelectedCourseIsPublished { get; set; }


        public int TotalLessons { get; set; }

        public int TotalStudents { get; set; }

        public int CompletedStudents { get; set; }

        public int LearningStudents { get; set; }

        public int NotStartedStudents { get; set; }

        public decimal AverageProgress { get; set; }


        public List<InstructorStudentItemViewModel>
            Students
        { get; set; }
                = new List<InstructorStudentItemViewModel>();
    }


    public class InstructorStudentCourseOptionViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public bool IsPublished { get; set; }
    }


    public class InstructorStudentItemViewModel
    {
        public string UserId { get; set; }
            = string.Empty;

        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public DateTime EnrolledAt { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        public int CompletedLessons { get; set; }

        public int TotalLessons { get; set; }

        public decimal Progress { get; set; }

        public string Status { get; set; }
            = "NotStarted";
    }
}
