using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================
        // DANH SÁCH KHÓA HỌC
        // =========================

        public async Task<IActionResult> Index(
            string? search,
            int? categoryId)
        {
            var coursesQuery = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.IsPublished)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                coursesQuery = coursesQuery.Where(c =>
                    c.Title.Contains(search) ||
                    (c.ShortDescription != null &&
                     c.ShortDescription.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                coursesQuery = coursesQuery.Where(c =>
                    c.CategoryId == categoryId.Value);
            }

            var courses = await coursesQuery
                .OrderByDescending(c => c.IsFeatured)
                .ThenByDescending(c => c.PublishedAt)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(courses);
        }


        // =========================
        // CHI TIẾT KHÓA HỌC
        // =========================

        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules
                    .OrderBy(m => m.DisplayOrder))
                    .ThenInclude(m => m.Lessons
                        .OrderBy(l => l.DisplayOrder))
                .Include(c => c.Reviews
                    .Where(r => r.IsApproved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c =>
                    c.Slug == slug &&
                    c.IsPublished);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra Student đã đăng ký khóa học chưa
            ViewBag.IsEnrolled = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    ViewBag.IsEnrolled =
                        await _context.Enrollments.AnyAsync(e =>
                            e.UserId == userId &&
                            e.CourseId == course.Id);
                }
            }

            // Tăng lượt xem
            course.ViewCount++;

            await _context.SaveChangesAsync();

            return View(course);
        }


        // =========================
        // ĐĂNG KÝ KHÓA HỌC
        // =========================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c =>
                    c.Id == courseId &&
                    c.IsPublished);

            if (course == null)
            {
                return NotFound();
            }

            // Không cho đăng ký trùng
            var alreadyEnrolled =
                await _context.Enrollments.AnyAsync(e =>
                    e.UserId == userId &&
                    e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["InfoMessage"] =
                    "Bạn đã đăng ký khóa học này.";

                return RedirectToAction(
                    "Details",
                    new { slug = course.Slug });
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                Progress = 0,
                LastAccessedAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);

            course.EnrollmentCount++;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Đăng ký khóa học thành công!";

            return RedirectToAction(
                "Details",
                new { slug = course.Slug });
        }
    }
}