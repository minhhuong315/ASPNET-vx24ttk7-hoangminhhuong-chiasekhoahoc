using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var wishlists =
                await _context.Wishlists
                    .AsNoTracking()
                    .Where(w =>
                        w.UserId == userId &&
                        w.Course.IsPublished)
                    .Include(w => w.Course)
                        .ThenInclude(c => c.Category)
                    .Include(w => w.Course)
                        .ThenInclude(c => c.Instructor)
                    .OrderByDescending(w => w.AddedAt)
                    .ToListAsync();

            return View(wishlists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int courseId,
            string? slug)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var course =
                await _context.Courses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == courseId &&
                        c.IsPublished);

            if (course == null)
            {
                return NotFound();
            }

            bool alreadyExists =
                await _context.Wishlists
                    .AnyAsync(w =>
                        w.UserId == userId &&
                        w.CourseId == courseId);

            if (!alreadyExists)
            {
                _context.Wishlists.Add(
                    new Wishlist
                    {
                        UserId = userId,
                        CourseId = courseId,
                        AddedAt = DateTime.UtcNow
                    });

                await _context.SaveChangesAsync();

                TempData["WishlistSuccess"] =
                    "Đã thêm khóa học vào danh sách yêu thích.";
            }
            else
            {
                TempData["WishlistInfo"] =
                    "Khóa học này đã có trong danh sách yêu thích.";
            }

            return RedirectToAction(
                "Details",
                "Courses",
                new
                {
                    slug =
                        string.IsNullOrWhiteSpace(slug)
                            ? course.Slug
                            : slug
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int courseId,
            string? slug,
            bool returnToDetails = false)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var wishlist =
                await _context.Wishlists
                    .FirstOrDefaultAsync(w =>
                        w.UserId == userId &&
                        w.CourseId == courseId);

            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();

                TempData["WishlistSuccess"] =
                    "Đã xóa khóa học khỏi danh sách yêu thích.";
            }

            if (returnToDetails)
            {
                var courseSlug =
                    !string.IsNullOrWhiteSpace(slug)
                        ? slug
                        : await _context.Courses
                            .Where(c => c.Id == courseId)
                            .Select(c => c.Slug)
                            .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(courseSlug))
                {
                    return RedirectToAction(
                        "Details",
                        "Courses",
                        new { slug = courseSlug });
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
