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
        // DANH SÁCH KHÓA HỌC
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
        // CREATE COURSE - GET
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
        // CREATE COURSE - POST
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


            await ValidateCourseInputAsync(
                model.CategoryId,
                model.Level,
                model.Price,
                model.DiscountPrice);


            if (!ModelState.IsValid)
            {
                model.Categories =
                    await GetCategoryOptionsAsync();

                return View(model);
            }


            var slug =
                await BuildUniqueSlugAsync(
                    model.Title);


            var course =
                new Course
                {
                    Title =
                        model.Title.Trim(),

                    Slug =
                        slug,

                    ShortDescription =
                        CleanNullable(
                            model.ShortDescription),

                    Description =
                        CleanNullable(
                            model.Description),

                    Price =
                        model.Price,

                    DiscountPrice =
                        model.DiscountPrice,

                    ThumbnailUrl =
                        CleanNullable(
                            model.ThumbnailUrl),

                    VideoPreviewUrl =
                        CleanNullable(
                            model.VideoPreviewUrl),

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
                "Khóa học đã được tạo thành công.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = course.Id
                });
        }


        // =========================================
        // EDIT COURSE - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Edit(
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
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.InstructorId == userId);


            if (course == null)
            {
                return NotFound();
            }


            var model =
                new InstructorCourseEditViewModel
                {
                    Id =
                        course.Id,

                    Title =
                        course.Title,

                    ShortDescription =
                        course.ShortDescription,

                    Description =
                        course.Description,

                    CategoryId =
                        course.CategoryId,

                    Level =
                        course.Level,

                    Price =
                        course.Price,

                    DiscountPrice =
                        course.DiscountPrice,

                    ThumbnailUrl =
                        course.ThumbnailUrl,

                    VideoPreviewUrl =
                        course.VideoPreviewUrl,

                    Categories =
                        await GetCategoryOptionsAsync()
                };


            return View(model);
        }


        // =========================================
        // EDIT COURSE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            InstructorCourseEditViewModel model)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var course =
                await _context.Courses
                    .FirstOrDefaultAsync(c =>
                        c.Id == model.Id &&
                        c.InstructorId == userId);


            if (course == null)
            {
                return NotFound();
            }


            await ValidateCourseInputAsync(
                model.CategoryId,
                model.Level,
                model.Price,
                model.DiscountPrice);


            if (!ModelState.IsValid)
            {
                model.Categories =
                    await GetCategoryOptionsAsync();

                return View(model);
            }


            course.Title =
                model.Title.Trim();

            course.ShortDescription =
                CleanNullable(
                    model.ShortDescription);

            course.Description =
                CleanNullable(
                    model.Description);

            course.CategoryId =
                model.CategoryId;

            course.Level =
                model.Level;

            course.Price =
                model.Price;

            course.DiscountPrice =
                model.DiscountPrice;

            course.ThumbnailUrl =
                CleanNullable(
                    model.ThumbnailUrl);

            course.VideoPreviewUrl =
                CleanNullable(
                    model.VideoPreviewUrl);

            course.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            TempData["InstructorCourseSuccess"] =
                "Thông tin khóa học đã được cập nhật.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = course.Id
                });
        }


        // =========================================
        // MANAGE COURSE
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
        // CREATE MODULE - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> CreateModule(
            int courseId)
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
                    .FirstOrDefaultAsync(c =>
                        c.Id == courseId &&
                        c.InstructorId == userId);


            if (course == null)
            {
                return NotFound();
            }


            var model =
                new InstructorModuleCreateViewModel
                {
                    CourseId =
                        course.Id,

                    CourseTitle =
                        course.Title
                };


            return View(model);
        }


        // =========================================
        // CREATE MODULE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModule(
            InstructorModuleCreateViewModel model)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var course =
                await _context.Courses
                    .FirstOrDefaultAsync(c =>
                        c.Id == model.CourseId &&
                        c.InstructorId == userId);


            if (course == null)
            {
                return NotFound();
            }


            model.CourseTitle =
                course.Title;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            int currentMaxOrder =
                await _context.Modules
                    .Where(m =>
                        m.CourseId == course.Id)
                    .Select(m =>
                        (int?)m.DisplayOrder)
                    .MaxAsync()
                ?? 0;


            var module =
                new Module
                {
                    CourseId =
                        course.Id,

                    Title =
                        model.Title.Trim(),

                    Description =
                        CleanNullable(
                            model.Description),

                    DisplayOrder =
                        currentMaxOrder + 1,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.Modules.Add(module);

            course.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            TempData["InstructorCourseSuccess"] =
                "Module mới đã được thêm vào khóa học.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = course.Id
                });
        }


        // =========================================
        // EDIT MODULE - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> EditModule(
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
                    .AsNoTracking()
                    .Include(m =>
                        m.Course)
                    .FirstOrDefaultAsync(m =>
                        m.Id == id &&
                        m.Course.InstructorId == userId);


            if (module == null)
            {
                return NotFound();
            }


            var model =
                new InstructorModuleEditViewModel
                {
                    Id =
                        module.Id,

                    CourseId =
                        module.CourseId,

                    CourseTitle =
                        module.Course.Title,

                    Title =
                        module.Title,

                    Description =
                        module.Description,

                    DisplayOrder =
                        module.DisplayOrder
                };


            return View(model);
        }


        // =========================================
        // EDIT MODULE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModule(
            InstructorModuleEditViewModel model)
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
                    .FirstOrDefaultAsync(m =>
                        m.Id == model.Id &&
                        m.CourseId == model.CourseId &&
                        m.Course.InstructorId == userId);


            if (module == null)
            {
                return NotFound();
            }


            model.CourseTitle =
                module.Course.Title;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            module.Title =
                model.Title.Trim();

            module.Description =
                CleanNullable(
                    model.Description);

            module.DisplayOrder =
                model.DisplayOrder;

            module.Course.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            TempData["InstructorCourseSuccess"] =
                "Module đã được cập nhật.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = module.CourseId
                });
        }


        // =========================================
        // CREATE LESSON - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> CreateLesson(
            int moduleId)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var module =
                await _context.Modules
                    .AsNoTracking()
                    .Include(m =>
                        m.Course)
                    .FirstOrDefaultAsync(m =>
                        m.Id == moduleId &&
                        m.Course.InstructorId == userId);


            if (module == null)
            {
                return NotFound();
            }


            var model =
                new InstructorLessonCreateViewModel
                {
                    ModuleId =
                        module.Id,

                    CourseId =
                        module.CourseId,

                    CourseTitle =
                        module.Course.Title,

                    ModuleTitle =
                        module.Title,

                    Duration =
                        0,

                    IsFree =
                        false
                };


            return View(model);
        }


        // =========================================
        // CREATE LESSON - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(
            InstructorLessonCreateViewModel model)
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
                    .FirstOrDefaultAsync(m =>
                        m.Id == model.ModuleId &&
                        m.Course.InstructorId == userId);


            if (module == null)
            {
                return NotFound();
            }


            model.CourseId =
                module.CourseId;

            model.CourseTitle =
                module.Course.Title;

            model.ModuleTitle =
                module.Title;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            int currentMaxOrder =
                await _context.Lessons
                    .Where(l =>
                        l.ModuleId == module.Id)
                    .Select(l =>
                        (int?)l.DisplayOrder)
                    .MaxAsync()
                ?? 0;


            var lesson =
                new Lesson
                {
                    ModuleId =
                        module.Id,

                    Title =
                        model.Title.Trim(),

                    Content =
                        CleanNullable(
                            model.Content),

                    VideoUrl =
                        CleanNullable(
                            model.VideoUrl),

                    Duration =
                        model.Duration,

                    DisplayOrder =
                        currentMaxOrder + 1,

                    IsFree =
                        model.IsFree,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.Lessons.Add(lesson);


            await _context.SaveChangesAsync();


            await RecalculateCourseDurationAsync(
                module.Course);


            TempData["InstructorCourseSuccess"] =
                "Bài học mới đã được thêm vào module.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = module.CourseId
                });
        }


        // =========================================
        // EDIT LESSON - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> EditLesson(
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
                    .AsNoTracking()
                    .Include(l =>
                        l.Module)
                        .ThenInclude(m =>
                            m.Course)
                    .FirstOrDefaultAsync(l =>
                        l.Id == id &&
                        l.Module.Course.InstructorId == userId);


            if (lesson == null)
            {
                return NotFound();
            }


            var model =
                new InstructorLessonEditViewModel
                {
                    Id =
                        lesson.Id,

                    ModuleId =
                        lesson.ModuleId,

                    CourseId =
                        lesson.Module.CourseId,

                    CourseTitle =
                        lesson.Module.Course.Title,

                    ModuleTitle =
                        lesson.Module.Title,

                    Title =
                        lesson.Title,

                    Content =
                        lesson.Content,

                    VideoUrl =
                        lesson.VideoUrl,

                    Duration =
                        lesson.Duration,

                    DisplayOrder =
                        lesson.DisplayOrder,

                    IsFree =
                        lesson.IsFree
                };


            return View(model);
        }


        // =========================================
        // EDIT LESSON - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(
            InstructorLessonEditViewModel model)
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
                        l.Id == model.Id &&
                        l.ModuleId == model.ModuleId &&
                        l.Module.Course.InstructorId == userId);


            if (lesson == null)
            {
                return NotFound();
            }


            model.CourseId =
                lesson.Module.CourseId;

            model.CourseTitle =
                lesson.Module.Course.Title;

            model.ModuleTitle =
                lesson.Module.Title;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            lesson.Title =
                model.Title.Trim();

            lesson.Content =
                CleanNullable(
                    model.Content);

            lesson.VideoUrl =
                CleanNullable(
                    model.VideoUrl);

            lesson.Duration =
                model.Duration;

            lesson.DisplayOrder =
                model.DisplayOrder;

            lesson.IsFree =
                model.IsFree;


            await _context.SaveChangesAsync();


            await RecalculateCourseDurationAsync(
                lesson.Module.Course);


            TempData["InstructorCourseSuccess"] =
                "Bài học đã được cập nhật.";


            return RedirectToAction(
                nameof(Manage),
                new
                {
                    id = lesson.Module.CourseId
                });
        }


        // =========================================
        // TÍNH LẠI THỜI LƯỢNG KHÓA HỌC
        // =========================================

        private async Task RecalculateCourseDurationAsync(
            Course course)
        {
            int totalDuration =
                await _context.Lessons
                    .Where(l =>
                        l.Module.CourseId == course.Id)
                    .Select(l =>
                        (int?)l.Duration)
                    .SumAsync()
                ?? 0;


            course.Duration =
                totalDuration;

            course.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();
        }


        // =========================================
        // COURSE VALIDATION
        // =========================================

        private async Task ValidateCourseInputAsync(
            int categoryId,
            string level,
            decimal price,
            decimal? discountPrice)
        {
            string[] allowedLevels =
            {
                "Beginner",
                "Intermediate",
                "Advanced"
            };


            if (!allowedLevels.Contains(level))
            {
                ModelState.AddModelError(
                    "Level",
                    "Cấp độ khóa học không hợp lệ.");
            }


            bool categoryExists =
                await _context.Categories
                    .AnyAsync(c =>
                        c.Id == categoryId);


            if (!categoryExists)
            {
                ModelState.AddModelError(
                    "CategoryId",
                    "Danh mục đã chọn không tồn tại.");
            }


            if (discountPrice.HasValue &&
                discountPrice.Value > 0 &&
                discountPrice.Value >= price)
            {
                ModelState.AddModelError(
                    "DiscountPrice",
                    "Giá khuyến mãi phải nhỏ hơn giá gốc.");
            }
        }


        // =========================================
        // CATEGORY
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
        // CLEAN STRING
        // =========================================

        private static string? CleanNullable(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        // =========================================
        // UNIQUE SLUG
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
        // GENERATE SLUG
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
                var category =
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            character);


                if (category !=
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