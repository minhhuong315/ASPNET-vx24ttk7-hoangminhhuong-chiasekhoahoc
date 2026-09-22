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

            ViewBag.IsEnrolled = false;
            ViewBag.EnrollmentProgress = 0m;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var enrollment = await _context.Enrollments
                        .FirstOrDefaultAsync(e =>
                            e.UserId == userId &&
                            e.CourseId == course.Id);

                    if (enrollment != null)
                    {
                        ViewBag.IsEnrolled = true;
                        ViewBag.EnrollmentProgress =
                            Math.Clamp(
                                enrollment.Progress,
                                0m,
                                100m);
                    }
                }
            }

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

        // =========================
        // KHÓA HỌC CỦA TÔI
        // =========================
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .OrderByDescending(e =>
                    e.LastAccessedAt ?? e.EnrolledAt)
                .ToListAsync();

            return View(enrollments);
        }
    }
}