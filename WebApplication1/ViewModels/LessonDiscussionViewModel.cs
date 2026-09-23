using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.ViewModels
{
    public class LessonDiscussionViewModel
    {
        public int LessonId { get; set; }

        public int CourseId { get; set; }

        public List<LessonQuestion> Questions { get; set; }
            = new List<LessonQuestion>();
    }
}