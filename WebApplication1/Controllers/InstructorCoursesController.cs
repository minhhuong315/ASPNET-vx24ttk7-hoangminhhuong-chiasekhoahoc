using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public InstructorCoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // KHÓA HỌC ĐANG PHỤ TRÁCH
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            /*
             * Chỉ lấy khóa học thuộc
             * đúng Instructor đang đăng nhập.
             */
            var courses =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.InstructorId == userId)
                    .Include(c =>
                        c.Category)
                    .Include(c =>
                        c.Modules)
                        .ThenInclude(m =>
                            m.Lessons)
                    .OrderByDescending(c =>
                        c.UpdatedAt)
                    .ThenBy(c =>
                        c.Title)
                    .ToListAsync();


            var courseItems =
                courses
                    .Select(c =>
                        new InstructorCourseItemViewModel
                        {
                            Id =
                                c.Id,

                            Title =
                                c.Title,

                            Slug =
                                c.Slug,

                            CategoryName =
                                c.Category?.Name
                                ?? "Chưa phân loại",

                            CategoryIcon =
                                c.Category?.Icon
                                ?? "IT",

                            Level =
                                c.Level,

                            Price =
                                c.Price,

                            IsPublished =
                                c.IsPublished,

                            ModuleCount =
                                c.Modules.Count,

                            LessonCount =
                                c.Modules.Sum(m =>
                                    m.Lessons.Count),

                            EnrollmentCount =
                                c.EnrollmentCount,

                            UpdatedAt =
                                c.UpdatedAt
                        })
                    .ToList();


            var model =
                new InstructorCoursesViewModel
                {
                    Courses =
                        courseItems,

                    TotalCourses =
                        courseItems.Count,

                    PublishedCourses =
                        courseItems.Count(c =>
                            c.IsPublished),

                    DraftCourses =
                        courseItems.Count(c =>
                            !c.IsPublished),

                    TotalLessons =
                        courseItems.Sum(c =>
                            c.LessonCount),

                    TotalEnrollments =
                        courseItems.Sum(c =>
                            c.EnrollmentCount)
                };


            return View(model);
        }


        // =========================================
        // QUẢN LÝ NỘI DUNG MỘT KHÓA HỌC
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Manage(
            int id)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            /*
             * Kiểm tra quyền sở hữu ngay trong query.
             *
             * Instructor không thể thay id trên URL
             * để xem trang quản lý khóa của người khác.
             */
            var course =
                await _context.Courses
                    .AsNoTracking()
                    .Include(c =>
                        c.Category)
                    .Include(c =>
                        c.Modules
                            .OrderBy(m =>
                                m.DisplayOrder))
                        .ThenInclude(m =>
                            m.Lessons
                                .OrderBy(l =>
                                    l.DisplayOrder))
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.InstructorId == userId);


            if (course == null)
            {
                return NotFound();
            }


            return View(course);
        }
    }
}