namespace OnlineLearningPlatform.ViewModels
{
    public class InstructorsPageViewModel
    {
        public List<InstructorPublicItemViewModel>
            Instructors
        { get; set; }
                = new List<InstructorPublicItemViewModel>();
    }


    public class InstructorPublicItemViewModel
    {
        public string Id { get; set; }
            = string.Empty;

        public string FullName { get; set; }
            = string.Empty;

        public string? Bio { get; set; }

        public int PublishedCourseCount { get; set; }

        public int StudentCount { get; set; }
    }
}