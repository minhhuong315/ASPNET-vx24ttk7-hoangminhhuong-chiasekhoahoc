using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // TRANG CHỦ
        // =========================================

        public async Task<IActionResult> Index()
        {
            var model =
                new HomeIndexViewModel();


            // =====================================
            // CHỈ LẤY DASHBOARD HỌC TẬP
            // NẾU USER LÀ STUDENT
            // =====================================

            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Student"))
            {
                var userId =
                    _userManager.GetUserId(User);


                if (!string.IsNullOrWhiteSpace(userId))
                {
                    // =================================
                    // 1. LẤY TẤT CẢ KHÓA ĐÃ ĐĂNG KÝ
                    // =================================

                    var enrollments =
                        await _context.Enrollments
                            .AsNoTracking()
                            .Where(e =>
                                e.UserId == userId)
                            .Include(e =>
                                e.Course)
                                .ThenInclude(c =>
                                    c.Modules)
                                    .ThenInclude(m =>
                                        m.Lessons)
                            .OrderByDescending(e =>
                                e.EnrolledAt)
                            .ToListAsync();


                    // =================================
                    // 2. TỔNG SỐ KHÓA ĐÃ ĐĂNG KÝ
                    // =================================

                    model.RegisteredCourseCount =
                        enrollments.Count;


                    // =================================
                    // 3. LẤY TẤT CẢ LESSON ID
                    // =================================

                    var allLessonIds =
                        enrollments
                            .SelectMany(e =>
                                e.Course.Modules)
                            .SelectMany(m =>
                                m.Lessons)
                            .Select(l =>
                                l.Id)
                            .Distinct()
                            .ToList();


                    // =================================
                    // 4. LẤY CÁC BÀI ĐÃ HOÀN THÀNH
                    // =================================

                    var completedLessonIds =
                        await _context.LessonProgresses
                            .AsNoTracking()
                            .Where(p =>
                                p.UserId == userId &&
                                p.IsCompleted &&
                                allLessonIds.Contains(
                                    p.LessonId))
                            .Select(p =>
                                p.LessonId)
                            .ToListAsync();


                    var completedLessonSet =
                        completedLessonIds
                            .ToHashSet();


                    model.CompletedLessonCount =
                        completedLessonSet.Count;


                    // =================================
                    // 5. TÍNH PROGRESS THẬT
                    //    CHO TỪNG KHÓA HỌC
                    // =================================

                    var courseModels =
                        new List<HomeCourseViewModel>();


                    foreach (var enrollment in enrollments)
                    {
                        var course =
                            enrollment.Course;


                        /*
                         * Sắp xếp lesson đúng thứ tự:
                         *
                         * Module.DisplayOrder
                         *          ↓
                         * Lesson.DisplayOrder
                         */
                        var orderedLessons =
                            course.Modules
                                .OrderBy(m =>
                                    m.DisplayOrder)
                                .SelectMany(m =>
                                    m.Lessons
                                        .OrderBy(l =>
                                            l.DisplayOrder))
                                .ToList();


                        var totalLessonCount =
                            orderedLessons.Count;


                        var completedCount =
                            orderedLessons.Count(l =>
                                completedLessonSet.Contains(
                                    l.Id));


                        decimal progress = 0;


                        if (totalLessonCount > 0)
                        {
                            progress =
                                Math.Round(
                                    (decimal)completedCount /
                                    totalLessonCount *
                                    100,
                                    0);
                        }


                        // =================================
                        // BÀI TIẾP THEO ĐƯỢC HỌC
                        // =================================

                        var firstIncompleteLesson =
                            orderedLessons
                                .FirstOrDefault(l =>
                                    !completedLessonSet.Contains(
                                        l.Id));


                        int? continueLessonId;


                        /*
                         * Chưa hoàn thành hết:
                         * → vào bài đầu tiên chưa hoàn thành.
                         *
                         * Đã hoàn thành 100%:
                         * → vào bài đầu tiên để xem lại.
                         */
                        if (firstIncompleteLesson != null)
                        {
                            continueLessonId =
                                firstIncompleteLesson.Id;
                        }
                        else
                        {
                            continueLessonId =
                                orderedLessons
                                    .FirstOrDefault()
                                    ?.Id;
                        }


                        courseModels.Add(
                            new HomeCourseViewModel
                            {
                                CourseId =
                                    course.Id,

                                Title =
                                    course.Title,

                                Slug =
                                    course.Slug,

                                Progress =
                                    progress,

                                CompletedLessonCount =
                                    completedCount,

                                TotalLessonCount =
                                    totalLessonCount,

                                ContinueLessonId =
                                    continueLessonId
                            });
                    }


                    // =================================
                    // 6. TÍNH TIẾN ĐỘ TRUNG BÌNH
                    // =================================

                    if (courseModels.Count > 0)
                    {
                        model.AverageProgress =
                            Math.Round(
                                courseModels
                                    .Average(c =>
                                        c.Progress),
                                0);
                    }


                    // =================================
                    // 7. HOME CHỈ HIỆN 2 KHÓA
                    //    ĐĂNG KÝ GẦN NHẤT
                    // =================================

                    model.StudentCourses =
                        courseModels
                            .Take(2)
                            .ToList();
                }
            }


            return View(model);
        }


        // =========================================
        // PRIVACY
        // =========================================

        public IActionResult Privacy()
        {
            return View();
        }


        // =========================================
        // ERROR
        // =========================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}