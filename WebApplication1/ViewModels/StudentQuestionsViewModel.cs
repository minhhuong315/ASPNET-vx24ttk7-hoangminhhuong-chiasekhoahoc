using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.ViewModels
{
    public class StudentQuestionsViewModel
    {
        public List<LessonQuestion> Questions { get; set; }
            = new List<LessonQuestion>();

        public int TotalQuestions { get; set; }

        public int WaitingQuestions { get; set; }

        public int AnsweredQuestions { get; set; }

        public string Filter { get; set; }
            = "all";

        public int? SelectedQuestionId { get; set; }
    }
}