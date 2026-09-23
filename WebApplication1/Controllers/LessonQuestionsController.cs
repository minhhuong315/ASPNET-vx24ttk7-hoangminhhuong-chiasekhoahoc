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


        // =========================================
        // STUDENT ĐẶT CÂU HỎI
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(
            int lessonId,
            string? content)
        {
            // =====================================
            // 1. LẤY STUDENT ĐANG ĐĂNG NHẬP
            // =====================================

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


            // =====================================
            // 2. LẤY BÀI HỌC
            // =====================================

            var lesson =
                await _context.Lessons
                    .Include(l => l.Module)
                    .FirstOrDefaultAsync(l =>
                        l.Id == lessonId);


            if (lesson == null)
            {
                return NotFound();
            }


            var courseId =
                lesson.Module.CourseId;


            // =====================================
            // 3. LẤY GIẢNG VIÊN PHỤ TRÁCH KHÓA HỌC
            // =====================================

            var instructorId =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.Id == courseId)
                    .Select(c =>
                        c.InstructorId)
                    .FirstOrDefaultAsync();


            if (string.IsNullOrWhiteSpace(
                instructorId))
            {
                return NotFound();
            }


            // =====================================
            // 4. KIỂM TRA STUDENT ĐÃ ĐĂNG KÝ
            // =====================================

            var enrollment =
                await _context.Enrollments
                    .FirstOrDefaultAsync(e =>
                        e.UserId == userId &&
                        e.CourseId == courseId);


            if (enrollment == null)
            {
                return Forbid();
            }


            // =====================================
            // 5. KIỂM TRA BÀI ĐÃ MỞ KHÓA
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


            var targetIndex =
                lessonIds.IndexOf(
                    lessonId);


            if (targetIndex < 0)
            {
                return NotFound();
            }


            var firstIncompleteIndex =
                lessonIds.FindIndex(id =>
                    !completedLessonIds.Contains(
                        id));


            bool lessonAlreadyCompleted =
                completedLessonIds.Contains(
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
            // 6. KIỂM TRA NỘI DUNG CÂU HỎI
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
            // 7. TẠO CÂU HỎI
            // =====================================

            var question =
                new LessonQuestion
                {
                    LessonId = lessonId,

                    UserId = userId,

                    Content = content,

                    CreatedAt =
                        DateTime.UtcNow,

                    IsResolved = false
                };


            _context.LessonQuestions.Add(
                question);


            // =====================================
            // 8. TẠO THÔNG BÁO CHO GIẢNG VIÊN
            // =====================================

            /*
             * Ưu tiên hiển thị FullName.
             *
             * Nếu chưa có FullName:
             * dùng Email.
             *
             * Nếu cả hai đều không có:
             * hiển thị "Học viên".
             */

            var studentDisplayName =
                !string.IsNullOrWhiteSpace(
                    student.FullName)
                    ? student.FullName
                    : !string.IsNullOrWhiteSpace(
                        student.Email)
                        ? student.Email
                        : "Học viên";


            /*
             * Chỉ tạo notification nếu
             * người đặt câu hỏi không phải
             * chính giảng viên.
             *
             * Thực tế Ask chỉ cho Student,
             * nhưng kiểm tra thêm để an toàn.
             */

            if (instructorId != userId)
            {
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

                        /*
                         * Tạm thời chưa đặt RelatedUrl.
                         *
                         * Bước sau mình sẽ làm trang
                         * Hỏi đáp dành cho Instructor,
                         * lúc đó notification sẽ trỏ
                         * thẳng tới câu hỏi.
                         */
                        RelatedUrl = null,

                        IsRead = false,

                        ReadAt = null,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _context.Notifications.Add(
                    notification);
            }


            // =====================================
            // 9. LƯU QUESTION + NOTIFICATION
            //    TRONG CÙNG MỘT LẦN SAVE
            // =====================================

            await _context.SaveChangesAsync();


            // =====================================
            // 10. THÔNG BÁO CHO STUDENT
            // =====================================

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