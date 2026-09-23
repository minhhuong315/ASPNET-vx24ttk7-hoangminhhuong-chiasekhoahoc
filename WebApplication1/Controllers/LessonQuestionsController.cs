using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

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

        // =========================
        // STUDENT ĐẶT CÂU HỎI
        // =========================

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

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .FirstOrDefaultAsync(l =>
                    l.Id == lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            var courseId =
                lesson.Module.CourseId;

            // Student phải đăng ký khóa học
            var enrollment =
                await _context.Enrollments
                    .FirstOrDefaultAsync(e =>
                        e.UserId == userId &&
                        e.CourseId == courseId);

            if (enrollment == null)
            {
                return Forbid();
            }

            // =========================
            // KIỂM TRA BÀI ĐÃ MỞ KHÓA
            // =========================

            var lessonIds =
                await _context.Lessons
                    .Where(l =>
                        l.Module.CourseId == courseId)
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

            var targetIndex =
                lessonIds.IndexOf(lessonId);

            if (targetIndex < 0)
            {
                return NotFound();
            }

            var firstIncompleteIndex =
                lessonIds.FindIndex(id =>
                    !completedLessonIds.Contains(id));

            bool lessonAlreadyCompleted =
                completedLessonIds.Contains(
                    lessonId);

            bool lessonIsCurrentUnlockedLesson =
                firstIncompleteIndex >= 0 &&
                targetIndex == firstIncompleteIndex;

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
                        ? lessonIds[firstIncompleteIndex]
                        : lessonIds.First();

                return RedirectToAction(
                    "Learn",
                    "Courses",
                    new
                    {
                        courseId,
                        lessonId = availableLessonId
                    });
            }

            // =========================
            // KIỂM TRA NỘI DUNG
            // =========================

            content = content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
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

            // =========================
            // LƯU CÂU HỎI
            // =========================

            var question =
                new LessonQuestion
                {
                    LessonId = lessonId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                };

            _context.LessonQuestions.Add(
                question);

            await _context.SaveChangesAsync();

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
    }
}