using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class LearningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LearningController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // VIDEO KẾT THÚC -> HOÀN THÀNH BÀI HỌC
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteFromVideo(
            int lessonId)
        {
            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }


            // =========================
            // TÌM BÀI HỌC
            // =========================

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


            // =========================
            // KIỂM TRA ĐĂNG KÝ KHÓA
            // =========================

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
            // LẤY DANH SÁCH BÀI THEO
            // ĐÚNG THỨ TỰ KHÓA HỌC
            // =========================

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


            var targetIndex =
                lessonIds.IndexOf(lessonId);

            if (targetIndex < 0)
            {
                return NotFound();
            }


            // =========================
            // CÁC BÀI ĐÃ HOÀN THÀNH
            // =========================

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


            bool alreadyCompleted =
                completedLessonIds.Contains(
                    lessonId);


            var firstIncompleteIndex =
                lessonIds.FindIndex(id =>
                    !completedLessonIds.Contains(id));


            bool isCurrentUnlockedLesson =
                firstIncompleteIndex >= 0 &&
                targetIndex ==
                firstIncompleteIndex;


            /*
             * Bảo vệ phía server:
             * User không thể tự sửa lessonId
             * để hoàn thành bài chưa được mở khóa.
             */
            if (!alreadyCompleted &&
                !isCurrentUnlockedLesson)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Bài học này chưa được mở khóa."
                });
            }


            // =========================
            // LƯU LESSON PROGRESS
            // =========================

            if (!alreadyCompleted)
            {
                var lessonProgress =
                    await _context.LessonProgresses
                        .FirstOrDefaultAsync(p =>
                            p.UserId == userId &&
                            p.LessonId == lessonId);


                if (lessonProgress == null)
                {
                    lessonProgress =
                        new LessonProgress
                        {
                            UserId = userId,
                            LessonId = lessonId,

                            IsCompleted = true,

                            WatchedDuration =
                                lesson.Duration,

                            CompletedAt =
                                DateTime.UtcNow,

                            LastWatchedAt =
                                DateTime.UtcNow
                        };


                    _context.LessonProgresses.Add(
                        lessonProgress);
                }
                else
                {
                    lessonProgress.IsCompleted =
                        true;

                    lessonProgress.WatchedDuration =
                        lesson.Duration;

                    lessonProgress.CompletedAt ??=
                        DateTime.UtcNow;

                    lessonProgress.LastWatchedAt =
                        DateTime.UtcNow;
                }


                await _context.SaveChangesAsync();
            }


            // =========================
            // TÍNH LẠI TIẾN ĐỘ
            // =========================

            var completedCount =
                await _context.LessonProgresses
                    .CountAsync(p =>
                        p.UserId == userId &&
                        p.IsCompleted &&
                        lessonIds.Contains(
                            p.LessonId));


            decimal progress = 0m;


            if (lessonIds.Count > 0)
            {
                progress =
                    Math.Round(
                        (decimal)completedCount /
                        lessonIds.Count *
                        100m,
                        0,
                        MidpointRounding.AwayFromZero);
            }


            progress =
                Math.Clamp(
                    progress,
                    0m,
                    100m);


            enrollment.Progress =
                progress;

            enrollment.LastAccessedAt =
                DateTime.UtcNow;


            if (progress >= 100m)
            {
                enrollment.CompletedAt ??=
                    DateTime.UtcNow;
            }
            else
            {
                enrollment.CompletedAt = null;
            }


            await _context.SaveChangesAsync();


            // =========================
            // BÀI TIẾP THEO
            // =========================

            int? nextLessonId = null;


            if (targetIndex + 1 <
                lessonIds.Count)
            {
                nextLessonId =
                    lessonIds[
                        targetIndex + 1];
            }


            string? nextUrl = null;


            if (nextLessonId.HasValue)
            {
                nextUrl =
                    Url.Action(
                        "Learn",
                        "Courses",
                        new
                        {
                            courseId,
                            lessonId =
                                nextLessonId.Value
                        });
            }


            // =========================
            // TRẢ KẾT QUẢ CHO JAVASCRIPT
            // =========================

            return Json(new
            {
                success = true,

                alreadyCompleted,

                progress,

                courseCompleted =
                    progress >= 100m,

                nextLessonId,

                nextUrl,

                message =
                    progress >= 100m
                        ? "Chúc mừng! Bạn đã hoàn thành khóa học."
                        : alreadyCompleted
                            ? "Bài học này đã được hoàn thành."
                            : "Bạn đã hoàn thành bài học. Bài tiếp theo đã được mở khóa."
            });
        }
    }
}