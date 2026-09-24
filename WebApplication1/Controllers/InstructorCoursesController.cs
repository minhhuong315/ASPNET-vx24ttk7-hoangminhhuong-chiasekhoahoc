using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
        // TẠO KHÓA HỌC - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new InstructorCourseCreateViewModel
                {
                    Level =
                        "Beginner",

                    Price =
                        0,

                    Categories =
                        await GetCategoryOptionsAsync()
                };


            return View(model);
        }


        // =========================================
        // TẠO KHÓA HỌC - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InstructorCourseCreateViewModel model)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            // -------------------------------------
            // KIỂM TRA LEVEL
            // -------------------------------------

            string[] allowedLevels =
            {
                "Beginner",
                "Intermediate",
                "Advanced"
            };


            if (!allowedLevels.Contains(
                model.Level))
            {
                ModelState.AddModelError(
                    nameof(model.Level),
                    "Cấp độ khóa học không hợp lệ.");
            }


            // -------------------------------------
            // KIỂM TRA CATEGORY
            // -------------------------------------

            bool categoryExists =
                await _context.Categories
                    .AnyAsync(c =>
                        c.Id == model.CategoryId);


            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Danh mục đã chọn không tồn tại.");
            }


            // -------------------------------------
            // KIỂM TRA GIÁ KHUYẾN MÃI
            // -------------------------------------

            if (model.DiscountPrice.HasValue &&
                model.DiscountPrice.Value > 0 &&
                model.DiscountPrice.Value >= model.Price)
            {
                ModelState.AddModelError(
                    nameof(model.DiscountPrice),
                    "Giá khuyến mãi phải nhỏ hơn giá khóa học.");
            }


            // -------------------------------------
            // VALIDATION KHÔNG THÀNH CÔNG
            // -------------------------------------

            if (!ModelState.IsValid)
            {
                model.Categories =
                    await GetCategoryOptionsAsync();


                return View(model);
            }


            // -------------------------------------
            // SLUG DUY NHẤT
            // -------------------------------------

            var slug =
                await BuildUniqueSlugAsync(
                    model.Title);


            // -------------------------------------
            // TẠO COURSE
            // -------------------------------------

            var course =
                new Course
                {
                    Title =
                        model.Title.Trim(),

                    Slug =
                        slug,

                    ShortDescription =
                        string.IsNullOrWhiteSpace(
                            model.ShortDescription)
                            ? null
                            : model.ShortDescription.Trim(),

                    Description =
                        string.IsNullOrWhiteSpace(
                            model.Description)
                            ? null
                            : model.Description.Trim(),

                    Price =
                        model.Price,

                    DiscountPrice =
                        model.DiscountPrice,

                    ThumbnailUrl =
                        string.IsNullOrWhiteSpace(
                            model.ThumbnailUrl)
                            ? null
                            : model.ThumbnailUrl.Trim(),

                    VideoPreviewUrl =
                        string.IsNullOrWhiteSpace(
                            model.VideoPreviewUrl)
                            ? null
                            : model.VideoPreviewUrl.Trim(),

                    Level =
                        model.Level,

                    Language =
                        "vi",

                    Duration =
                        0,

                    InstructorId =
                        userId,

                    CategoryId =
                        model.CategoryId,

                    CreatedAt =
                        DateTime.UtcNow,

                    UpdatedAt =
                        DateTime.UtcNow,

                    PublishedAt =
                        null,

                    IsPublished =
                        false,

                    IsFeatured =
                        false,

                    ViewCount =
                        0,

                    EnrollmentCount =
                        0
                };


            _context.Courses.Add(course);

            await _context.SaveChangesAsync();


            TempData["InstructorCourseSuccess"] =
                "Khóa học đã được tạo và lưu ở trạng thái Bản nháp.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = course.Id
                });
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


        // =========================================
        // CATEGORY OPTIONS
        // =========================================

        private async Task<
            List<InstructorCourseCategoryOptionViewModel>>
            GetCategoryOptionsAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c =>
                    c.DisplayOrder)
                .ThenBy(c =>
                    c.Name)
                .Select(c =>
                    new InstructorCourseCategoryOptionViewModel
                    {
                        Id =
                            c.Id,

                        Name =
                            c.Name
                    })
                .ToListAsync();
        }


        // =========================================
        // SLUG DUY NHẤT
        // =========================================

        private async Task<string> BuildUniqueSlugAsync(
            string title)
        {
            var baseSlug =
                GenerateSlug(title);


            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug =
                    "khoa-hoc";
            }


            var slug =
                baseSlug;

            int number =
                2;


            while (await _context.Courses
                .AnyAsync(c =>
                    c.Slug == slug))
            {
                slug =
                    $"{baseSlug}-{number}";

                number++;
            }


            return slug;
        }


        // =========================================
        // TẠO SLUG TIẾNG VIỆT
        // =========================================

        private static string GenerateSlug(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            text =
                text.Trim()
                    .Replace("đ", "d")
                    .Replace("Đ", "D");


            string normalized =
                text.Normalize(
                    NormalizationForm.FormD);


            var builder =
                new StringBuilder();


            foreach (char character in normalized)
            {
                var unicodeCategory =
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            character);


                if (unicodeCategory !=
                    UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }


            string slug =
                builder
                    .ToString()
                    .Normalize(
                        NormalizationForm.FormC)
                    .ToLowerInvariant();


            slug =
                Regex.Replace(
                    slug,
                    @"[^a-z0-9]+",
                    "-");


            return slug.Trim('-');
        }
    }
}