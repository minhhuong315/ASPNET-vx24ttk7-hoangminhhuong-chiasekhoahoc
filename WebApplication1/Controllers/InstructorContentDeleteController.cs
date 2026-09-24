using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorContentDeleteController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser>
            _userManager;


        public InstructorContentDeleteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;

            _userManager = userManager;
        }


        // =========================================
        // XÓA BÀI HỌC
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(
            int id)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var lesson =
                await _context.Lessons
                    .Include(l =>
                        l.Module)
                        .ThenInclude(m =>
                            m.Course)
                    .FirstOrDefaultAsync(l =>
                        l.Id == id &&
                        l.Module.Course.InstructorId ==
                        userId);


            if (lesson == null)
            {
                return NotFound();
            }


            var course =
                lesson.Module.Course;


            // =====================================
            // KHÔNG XÓA KHI ĐANG PUBLISH
            // =====================================

            if (course.IsPublished)
            {
                TempData["InstructorCourseError"] =
                    "Không thể xóa bài học khi khóa học đang được xuất bản. Hãy bỏ xuất bản khóa học trước.";


                return RedirectToAction(
                    "Manage",
                    "InstructorCourses",
                    new
                    {
                        id = course.Id
                    });
            }


            // =====================================
            // KIỂM TRA TIẾN ĐỘ HỌC VIÊN
            // =====================================

            bool hasProgress =
                await _context.LessonProgresses
                    .AnyAsync(p =>
                        p.LessonId == lesson.Id);


            if (hasProgress)
            {
                TempData["InstructorCourseError"] =
                    "Không thể xóa bài học này vì đã có dữ liệu tiến độ học tập của học viên.";


                return RedirectToAction(
                    "Manage",
                    "InstructorCourses",
                    new
                    {
                        id = course.Id
                    });
            }


            // =====================================
            // KIỂM TRA Q&A
            // =====================================

            bool hasQuestions =
                await _context.LessonQuestions
                    .AnyAsync(q =>
                        q.LessonId == lesson.Id);


            if (hasQuestions)
            {
                TempData["InstructorCourseError"] =
                    "Không thể xóa bài học này vì đã có câu hỏi hoặc trao đổi của học viên.";


                return RedirectToAction(
                    "Manage",
                    "InstructorCourses",
                    new
                    {
                        id = course.Id
                    });
            }


            int moduleId =
                lesson.ModuleId;


            // =====================================
            // LẤY CÁC BÀI CÒN LẠI ĐỂ ĐÁNH LẠI ORDER
            // =====================================

            var remainingLessons =
                await _context.Lessons
                    .Where(l =>
                        l.ModuleId == moduleId &&
                        l.Id != lesson.Id)
                    .OrderBy(l =>
                        l.DisplayOrder)
                    .ThenBy(l =>
                        l.Id)
                    .ToListAsync();


            for (int index = 0;
                 index < remainingLessons.Count;
                 index++)
            {
                remainingLessons[index]
                    .DisplayOrder =
                        index + 1;
            }


            // =====================================
            // TÍNH LẠI THỜI LƯỢNG KHÓA HỌC
            // =====================================

            int remainingDuration =
                await _context.Lessons
                    .Where(l =>
                        l.Module.CourseId ==
                        course.Id &&
                        l.Id != lesson.Id)
                    .Select(l =>
                        (int?)l.Duration)
                    .SumAsync()
                ?? 0;


            course.Duration =
                remainingDuration;

            course.UpdatedAt =
                DateTime.UtcNow;


            // =====================================
            // XÓA
            // =====================================

            _context.Lessons.Remove(
                lesson);


            await _context.SaveChangesAsync();


            TempData["InstructorCourseSuccess"] =
                "Bài học đã được xóa thành công.";


            return RedirectToAction(
                "Manage",
                "InstructorCourses",
                new
                {
                    id = course.Id
                });
        }


        // =========================================
        // XÓA MODULE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteModule(
            int id)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var module =
                await _context.Modules
                    .Include(m =>
                        m.Course)
                    .Include(m =>
                        m.Lessons)
                    .FirstOrDefaultAsync(m =>
                        m.Id == id &&
                        m.Course.InstructorId ==
                        userId);


            if (module == null)
            {
                return NotFound();
            }


            var course =
                module.Course;


            // =====================================
            // KHÔNG XÓA KHI ĐANG PUBLISH
            // =====================================

            if (course.IsPublished)
            {
                TempData["InstructorCourseError"] =
                    "Không thể xóa module khi khóa học đang được xuất bản. Hãy bỏ xuất bản khóa học trước.";


                return RedirectToAction(
                    "Manage",
                    "InstructorCourses",
                    new
                    {
                        id = course.Id
                    });
            }


            var lessonIds =
                module.Lessons
                    .Select(l =>
                        l.Id)
                    .ToList();


            // =====================================
            // KIỂM TRA DỮ LIỆU LIÊN QUAN
            // =====================================

            if (lessonIds.Any())
            {
                bool hasProgress =
                    await _context.LessonProgresses
                        .AnyAsync(p =>
                            lessonIds.Contains(
                                p.LessonId));


                if (hasProgress)
                {
                    TempData["InstructorCourseError"] =
                        "Không thể xóa module này vì một hoặc nhiều bài học đã có dữ liệu tiến độ của học viên.";


                    return RedirectToAction(
                        "Manage",
                        "InstructorCourses",
                        new
                        {
                            id = course.Id
                        });
                }


                bool hasQuestions =
                    await _context.LessonQuestions
                        .AnyAsync(q =>
                            lessonIds.Contains(
                                q.LessonId));


                if (hasQuestions)
                {
                    TempData["InstructorCourseError"] =
                        "Không thể xóa module này vì một hoặc nhiều bài học đã có câu hỏi hoặc trao đổi của học viên.";


                    return RedirectToAction(
                        "Manage",
                        "InstructorCourses",
                        new
                        {
                            id = course.Id
                        });
                }
            }


            // =====================================
            // ĐÁNH LẠI THỨ TỰ MODULE
            // =====================================

            var remainingModules =
                await _context.Modules
                    .Where(m =>
                        m.CourseId == course.Id &&
                        m.Id != module.Id)
                    .OrderBy(m =>
                        m.DisplayOrder)
                    .ThenBy(m =>
                        m.Id)
                    .ToListAsync();


            for (int index = 0;
                 index < remainingModules.Count;
                 index++)
            {
                remainingModules[index]
                    .DisplayOrder =
                        index + 1;
            }


            // =====================================
            // TÍNH LẠI DURATION
            // =====================================

            int remainingDuration =
                await _context.Lessons
                    .Where(l =>
                        l.Module.CourseId ==
                        course.Id &&
                        l.ModuleId != module.Id)
                    .Select(l =>
                        (int?)l.Duration)
                    .SumAsync()
                ?? 0;


            course.Duration =
                remainingDuration;

            course.UpdatedAt =
                DateTime.UtcNow;


            // =====================================
            // XÓA MODULE
            //
            // Module -> Lesson đang Cascade.
            // Nhưng chỉ tới đây sau khi kiểm tra
            // không có Progress/Q&A cần bảo toàn.
            // =====================================

            _context.Modules.Remove(
                module);


            await _context.SaveChangesAsync();


            TempData["InstructorCourseSuccess"] =
                "Module đã được xóa thành công.";


            return RedirectToAction(
                "Manage",
                "InstructorCourses",
                new
                {
                    id = course.Id
                });
        }
    }
}