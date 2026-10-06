namespace OnlineLearningPlatform.ViewModels
{
    public class StudyHistoryViewModel
    {
        public int TotalCourses { get; set; }

        public int InProgressCourses { get; set; }

        public int CompletedCourses { get; set; }

        public int CompletedLessons { get; set; }

        public List<StudyHistoryCourseViewModel> Courses { get; set; } = new();

        public List<StudyHistoryActivityViewModel> Activities { get; set; } = new();
    }


    public class StudyHistoryCourseViewModel
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal Progress { get; set; }

        public string StatusKey { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime LastActivityAt { get; set; }
    }


    public class StudyHistoryActivityViewModel
    {
        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public int CourseId { get; set; }

        public int? LessonId { get; set; }
    }
}
