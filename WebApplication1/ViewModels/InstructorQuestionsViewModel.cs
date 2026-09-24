using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.ViewModels
{
    public class InstructorQuestionsViewModel
    {
        // =========================================
        // DANH SÁCH CÂU HỎI
        // =========================================

        public List<LessonQuestion> Questions { get; set; }
            = new List<LessonQuestion>();


        // =========================================
        // THỐNG KÊ
        // =========================================

        public int TotalQuestions { get; set; }

        public int UnansweredQuestions { get; set; }

        public int AnsweredQuestions { get; set; }


        // =========================================
        // CÂU HỎI ĐƯỢC MỞ TỪ NOTIFICATION
        // =========================================

        public int? SelectedQuestionId { get; set; }
    }
}