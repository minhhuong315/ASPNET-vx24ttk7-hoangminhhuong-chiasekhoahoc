using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize]
    public class LessonQuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public LessonQuestionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // INSTRUCTOR / ADMIN
        // DANH SÁCH CÂU HỎI
        // =====================================================

        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Index(
            int? selectedQuestionId)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var query =
                _context.LessonQuestions
                    .AsNoTracking()
                    .Include(q => q.User)
                    .Include(q => q.Lesson)
                        .ThenInclude(l => l.Module)
                            .ThenInclude(m => m.Course)
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.User)
                    .AsQueryable();


            if (User.IsInRole("Instructor"))
            {
                query =
                    query.Where(q =>
                        q.Lesson.Module.Course.InstructorId ==
                        userId);
            }


            var questions =
                await query
                    .OrderBy(q => q.IsResolved)
                    .ThenByDescending(q => q.CreatedAt)
                    .ToListAsync();


            var model =
                new InstructorQuestionsViewModel
                {
                    Questions =
                        questions,

                    TotalQuestions =
                        questions.Count,

                    UnansweredQuestions =
                        questions.Count(q =>
                            !q.IsResolved),

                    AnsweredQuestions =
                        questions.Count(q =>
                            q.IsResolved),

                    SelectedQuestionId =
                        selectedQuestionId
                };


            return View(model);
        }


        // =====================================================
        // STUDENT
        // CÂU HỎI CỦA TÔI
        // =====================================================

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyQuestions(
            string? filter,
            int? selectedQuestionId)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            filter =
                filter?
                    .Trim()
                    .ToLowerInvariant();


            if (filter != "waiting" &&
                filter != "answered")
            {
                filter =
                    "all";
            }


            var allQuestions =
                await _context.LessonQuestions
                    .AsNoTracking()
                    .Where(q =>
                        q.UserId == userId)
                    .Include(q => q.User)
                    .Include(q => q.Lesson)
                        .ThenInclude(l => l.Module)
                            .ThenInclude(m => m.Course)
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.User)
                    .OrderByDescending(q =>
                        q.CreatedAt)
                    .ToListAsync();


            IEnumerable<LessonQuestion>
                filteredQuestions =
                    allQuestions;


            if (filter == "waiting")
            {
                filteredQuestions =
                    filteredQuestions.Where(q =>
                        !q.IsResolved);
            }


            if (filter == "answered")
            {
                filteredQuestions =
                    filteredQuestions.Where(q =>
                        q.IsResolved);
            }


            var model =
                new StudentQuestionsViewModel
                {
                    Questions =
                        filteredQuestions
                            .ToList(),

                    TotalQuestions =
                        allQuestions.Count,

                    WaitingQuestions =
                        allQuestions.Count(q =>
                            !q.IsResolved),

                    AnsweredQuestions =
                        allQuestions.Count(q =>
                            q.IsResolved),

                    Filter =
                        filter,

                    SelectedQuestionId =
                        selectedQuestionId
                };


            return View(model);
        }


        // =====================================================
        // STUDENT ĐẶT CÂU HỎI
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(
            int lessonId,
            string? content)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var student =
                await _userManager.GetUserAsync(User);


            if (student == null)
            {
                return Challenge();
            }


            var lesson =
                await _context.Lessons
                    .Include(l => l.Module)
                        .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(l =>
                        l.Id == lessonId);


            if (lesson == null)
            {
                return NotFound();
            }


            var course =
                lesson.Module.Course;


            var courseId =
                course.Id;


            var instructorId =
                course.InstructorId;


            if (string.IsNullOrWhiteSpace(
                instructorId))
            {
                return NotFound();
            }


            var enrollmentExists =
                await _context.Enrollments
                    .AnyAsync(e =>
                        e.UserId == userId &&
                        e.CourseId == courseId);


            if (!enrollmentExists)
            {
                return Forbid();
            }


            // =====================================
            // KIỂM TRA BÀI HỌC ĐÃ MỞ
            // =====================================

            var lessonIds =
                await _context.Lessons
                    .Where(l =>
                        l.Module.CourseId ==
                        courseId)
                    .OrderBy(l =>
                        l.Module.DisplayOrder)
                    .ThenBy(l =>
                        l.DisplayOrder)
                    .Select(l =>
                        l.Id)
                    .ToListAsync();


            var completedLessonIds =
                await _context.LessonProgresses
                    .Where(p =>
                        p.UserId == userId &&
                        p.IsCompleted &&
                        lessonIds.Contains(
                            p.LessonId))
                    .Select(p =>
                        p.LessonId)
                    .ToListAsync();


            var completedLessonSet =
                completedLessonIds.ToHashSet();


            var targetIndex =
                lessonIds.IndexOf(
                    lessonId);


            if (targetIndex < 0)
            {
                return NotFound();
            }


            var firstIncompleteIndex =
                lessonIds.FindIndex(id =>
                    !completedLessonSet.Contains(
                        id));


            bool lessonAlreadyCompleted =
                completedLessonSet.Contains(
                    lessonId);


            bool lessonIsCurrentUnlockedLesson =
                firstIncompleteIndex >= 0 &&
                targetIndex ==
                firstIncompleteIndex;


            bool allLessonsCompleted =
                firstIncompleteIndex < 0;


            if (!lessonAlreadyCompleted &&
                !lessonIsCurrentUnlockedLesson &&
                !allLessonsCompleted)
            {
                TempData["LearningInfoMessage"] =
                    "Bạn chưa thể đặt câu hỏi ở bài học chưa được mở khóa.";


                var availableLessonId =
                    firstIncompleteIndex >= 0
                        ? lessonIds[
                            firstIncompleteIndex]
                        : lessonIds.First();


                return RedirectToAction(
                    "Learn",
                    "Courses",
                    new
                    {
                        courseId,
                        lessonId =
                            availableLessonId
                    });
            }


            // =====================================
            // KIỂM TRA NỘI DUNG
            // =====================================

            content =
                content?.Trim();


            if (string.IsNullOrWhiteSpace(
                content))
            {
                TempData["LearningInfoMessage"] =
                    "Vui lòng nhập nội dung câu hỏi.";


                return RedirectToAction(
                    "Learn",
                    "Courses",
                    new
                    {
                        courseId,
                        lessonId
                    });
            }


            if (content.Length > 2000)
            {
                TempData["LearningInfoMessage"] =
                    "Câu hỏi không được vượt quá 2000 ký tự.";


                return RedirectToAction(
                    "Learn",
                    "Courses",
                    new
                    {
                        courseId,
                        lessonId
                    });
            }


            // =====================================
            // TẠO QUESTION
            // =====================================

            var question =
                new LessonQuestion
                {
                    LessonId =
                        lessonId,

                    UserId =
                        userId,

                    Content =
                        content,

                    CreatedAt =
                        DateTime.UtcNow,

                    IsResolved =
                        false
                };


            _context.LessonQuestions.Add(
                question);


            await _context.SaveChangesAsync();


            // =====================================
            // NOTIFICATION CHO INSTRUCTOR
            // =====================================

            var studentDisplayName =
                !string.IsNullOrWhiteSpace(
                    student.FullName)
                    ? student.FullName
                    : !string.IsNullOrWhiteSpace(
                        student.Email)
                        ? student.Email
                        : "Học viên";


            if (instructorId != userId)
            {
                var relatedUrl =
                    Url.Action(
                        "Index",
                        "LessonQuestions",
                        new
                        {
                            selectedQuestionId =
                                question.Id
                        });


                var notification =
                    new Notification
                    {
                        UserId =
                            instructorId,

                        Title =
                            "Có câu hỏi mới",

                        Message =
                            $"{studentDisplayName} đã đặt câu hỏi trong bài \"{lesson.Title}\".",

                        Type =
                            "NewQuestion",

                        RelatedUrl =
                            relatedUrl,

                        IsRead =
                            false,

                        ReadAt =
                            null,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _context.Notifications.Add(
                    notification);


                await _context.SaveChangesAsync();
            }


            TempData["LearningSuccessMessage"] =
                "Câu hỏi của bạn đã được gửi.";


            return RedirectToAction(
                "Learn",
                "Courses",
                new
                {
                    courseId,
                    lessonId
                });
        }


        // =====================================================
        // STUDENT SỬA QUESTION
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(
            int questionId,
            string? content,
            bool returnToMyQuestions = false)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var question =
                await _context.LessonQuestions
                    .Include(q => q.Lesson)
                        .ThenInclude(l => l.Module)
                    .FirstOrDefaultAsync(q =>
                        q.Id == questionId);


            if (question == null)
            {
                return NotFound();
            }


            if (question.UserId != userId)
            {
                return Forbid();
            }


            content =
                content?.Trim();


            if (string.IsNullOrWhiteSpace(
                content))
            {
                if (returnToMyQuestions)
                {
                    TempData[
                        "MyQuestionInfoMessage"] =
                        "Vui lòng nhập nội dung câu hỏi.";


                    return RedirectToAction(
                        "MyQuestions",
                        new
                        {
                            selectedQuestionId =
                                question.Id
                        });
                }


                TempData["LearningInfoMessage"] =
                    "Vui lòng nhập nội dung câu hỏi.";


                return RedirectToAction(
                    "Learn",
                    "Courses",
                    new
                    {
                        courseId =
                            question
                                .Lesson
                                .Module
                                .CourseId,

                        lessonId =
                            question.LessonId
                    });
            }


            if (content.Length > 2000)
            {
                if (returnToMyQuestions)
                {
                    TempData[
                        "MyQuestionInfoMessage"] =
                        "Câu hỏi không được vượt quá 2000 ký tự.";


                    return RedirectToAction(
                        "MyQuestions",
                        new
                        {
                            selectedQuestionId =
                                question.Id
                        });
                }


                TempData["LearningInfoMessage"] =
                    "Câu hỏi không được vượt quá 2000 ký tự.";


                return RedirectToAction(
                    "Learn",
                    "Courses",
                    new
                    {
                        courseId =
                            question
                                .Lesson
                                .Module
                                .CourseId,

                        lessonId =
                            question.LessonId
                    });
            }


            question.Content =
                content;


            await _context.SaveChangesAsync();


            if (returnToMyQuestions)
            {
                TempData[
                    "MyQuestionSuccessMessage"] =
                    "Câu hỏi đã được cập nhật.";


                return RedirectToAction(
                    "MyQuestions",
                    new
                    {
                        selectedQuestionId =
                            question.Id
                    });
            }


            TempData["LearningSuccessMessage"] =
                "Câu hỏi đã được cập nhật.";


            return RedirectToAction(
                "Learn",
                "Courses",
                new
                {
                    courseId =
                        question
                            .Lesson
                            .Module
                            .CourseId,

                    lessonId =
                        question.LessonId
                });
        }


        // =====================================================
        // STUDENT XÓA QUESTION
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(
            int questionId,
            bool returnToMyQuestions = false)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var question =
                await _context.LessonQuestions
                    .Include(q => q.Answers)
                    .Include(q => q.Lesson)
                        .ThenInclude(l => l.Module)
                    .FirstOrDefaultAsync(q =>
                        q.Id == questionId);


            if (question == null)
            {
                return NotFound();
            }


            if (question.UserId != userId)
            {
                return Forbid();
            }


            var courseId =
                question
                    .Lesson
                    .Module
                    .CourseId;


            var lessonId =
                question.LessonId;


            if (question.Answers.Any())
            {
                _context.LessonAnswers.RemoveRange(
                    question.Answers);
            }


            _context.LessonQuestions.Remove(
                question);


            await _context.SaveChangesAsync();


            if (returnToMyQuestions)
            {
                TempData[
                    "MyQuestionSuccessMessage"] =
                    "Câu hỏi đã được xóa.";


                return RedirectToAction(
                    "MyQuestions");
            }


            TempData["LearningSuccessMessage"] =
                "Câu hỏi đã được xóa.";


            return RedirectToAction(
                "Learn",
                "Courses",
                new
                {
                    courseId,
                    lessonId
                });
        }


        // =====================================================
        // INSTRUCTOR / ADMIN TRẢ LỜI
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Answer(
            int questionId,
            string? content)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var question =
                await _context.LessonQuestions
                    .Include(q => q.User)
                    .Include(q => q.Lesson)
                        .ThenInclude(l => l.Module)
                            .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(q =>
                        q.Id == questionId);


            if (question == null)
            {
                return NotFound();
            }


            var course =
                question
                    .Lesson
                    .Module
                    .Course;


            if (User.IsInRole("Instructor") &&
                course.InstructorId != userId)
            {
                return Forbid();
            }


            content =
                content?.Trim();


            if (string.IsNullOrWhiteSpace(
                content))
            {
                TempData["QuestionInfoMessage"] =
                    "Vui lòng nhập nội dung phản hồi.";


                return RedirectToAction(
                    "Index",
                    new
                    {
                        selectedQuestionId =
                            questionId
                    });
            }


            if (content.Length > 2000)
            {
                TempData["QuestionInfoMessage"] =
                    "Phản hồi không được vượt quá 2000 ký tự.";


                return RedirectToAction(
                    "Index",
                    new
                    {
                        selectedQuestionId =
                            questionId
                    });
            }


            var answer =
                new LessonAnswer
                {
                    QuestionId =
                        question.Id,

                    UserId =
                        userId,

                    Content =
                        content,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.LessonAnswers.Add(
                answer);


            question.IsResolved =
                true;


            // =====================================
            // QUAN TRỌNG:
            // NOTIFICATION STUDENT TRỎ VỀ
            // "CÂU HỎI CỦA TÔI"
            // =====================================

            var relatedUrl =
                Url.Action(
                    "MyQuestions",
                    "LessonQuestions",
                    new
                    {
                        selectedQuestionId =
                            question.Id
                    });


            var notification =
                new Notification
                {
                    UserId =
                        question.UserId,

                    Title =
                        "Câu hỏi đã được trả lời",

                    Message =
                        $"Giảng viên đã trả lời câu hỏi của bạn trong bài \"{question.Lesson.Title}\".",

                    Type =
                        "QuestionAnswered",

                    RelatedUrl =
                        relatedUrl,

                    IsRead =
                        false,

                    ReadAt =
                        null,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.Notifications.Add(
                notification);


            await _context.SaveChangesAsync();


            TempData["QuestionSuccessMessage"] =
                "Phản hồi đã được gửi cho học viên.";


            return RedirectToAction(
                "Index",
                new
                {
                    selectedQuestionId =
                        question.Id
                });
        }


        // =====================================================
        // INSTRUCTOR / ADMIN SỬA PHẢN HỒI
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnswer(
            int answerId,
            string? content)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var answer =
                await _context.LessonAnswers
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Lesson)
                            .ThenInclude(l => l.Module)
                                .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(a =>
                        a.Id == answerId);


            if (answer == null)
            {
                return NotFound();
            }


            var course =
                answer
                    .Question
                    .Lesson
                    .Module
                    .Course;


            if (User.IsInRole("Instructor"))
            {
                if (course.InstructorId != userId ||
                    answer.UserId != userId)
                {
                    return Forbid();
                }
            }


            content =
                content?.Trim();


            if (string.IsNullOrWhiteSpace(
                content))
            {
                TempData["QuestionInfoMessage"] =
                    "Vui lòng nhập nội dung phản hồi.";


                return RedirectToAction(
                    "Index",
                    new
                    {
                        selectedQuestionId =
                            answer.QuestionId
                    });
            }


            if (content.Length > 2000)
            {
                TempData["QuestionInfoMessage"] =
                    "Phản hồi không được vượt quá 2000 ký tự.";


                return RedirectToAction(
                    "Index",
                    new
                    {
                        selectedQuestionId =
                            answer.QuestionId
                    });
            }


            answer.Content =
                content;


            await _context.SaveChangesAsync();


            TempData["QuestionSuccessMessage"] =
                "Phản hồi đã được cập nhật.";


            return RedirectToAction(
                "Index",
                new
                {
                    selectedQuestionId =
                        answer.QuestionId
                });
        }


        // =====================================================
        // INSTRUCTOR / ADMIN XÓA PHẢN HỒI
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(
            int answerId)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var answer =
                await _context.LessonAnswers
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Answers)
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Lesson)
                            .ThenInclude(l => l.Module)
                                .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(a =>
                        a.Id == answerId);


            if (answer == null)
            {
                return NotFound();
            }


            var question =
                answer.Question;


            var course =
                question
                    .Lesson
                    .Module
                    .Course;


            if (User.IsInRole("Instructor"))
            {
                if (course.InstructorId != userId ||
                    answer.UserId != userId)
                {
                    return Forbid();
                }
            }


            bool hasOtherAnswers =
                question.Answers.Any(a =>
                    a.Id != answer.Id);


            _context.LessonAnswers.Remove(
                answer);


            question.IsResolved =
                hasOtherAnswers;


            await _context.SaveChangesAsync();


            TempData["QuestionSuccessMessage"] =
                hasOtherAnswers
                    ? "Phản hồi đã được xóa."
                    : "Phản hồi đã được xóa. Câu hỏi đã chuyển về trạng thái chưa trả lời.";


            return RedirectToAction(
                "Index",
                new
                {
                    selectedQuestionId =
                        question.Id
                });
        }
    }
}