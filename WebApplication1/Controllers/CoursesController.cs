using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;

namespace OnlineLearningPlatform.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
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

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                coursesQuery = coursesQuery.Where(c =>
                    c.Title.Contains(search) ||
                    (c.ShortDescription != null &&
                     c.ShortDescription.Contains(search)));
            }

            // Lọc danh mục
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

            // Tăng lượt xem
            course.ViewCount++;

            await _context.SaveChangesAsync();

            return View(course);
        }
    }
}