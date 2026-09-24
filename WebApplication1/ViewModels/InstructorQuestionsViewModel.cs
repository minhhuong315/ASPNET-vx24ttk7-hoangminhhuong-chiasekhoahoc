using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.ViewModels
{
    public class InstructorQuestionsViewModel
    {
        public List<LessonQuestion> Questions { get; set; }
            = new List<LessonQuestion>();

        public int TotalQuestions { get; set; }

        public int UnansweredQuestions { get; set; }

        public int AnsweredQuestions { get; set; }

        public int? SelectedQuestionId { get; set; }
    }
}