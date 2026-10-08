using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;


        public AdminCategoriesController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // DANH SÁCH DANH MỤC
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            search =
                search?.Trim();

            status =
                NormalizeStatus(
                    status);


            var query =
                _context.Categories
                    .AsNoTracking()
                    .AsQueryable();


            if (!string.IsNullOrWhiteSpace(
                search))
            {
                query =
                    query.Where(c =>
                        c.Name.Contains(search) ||
                        (
                            c.Slug != null &&
                            c.Slug.Contains(search)
                        ));
            }


            if (status == "active")
            {
                query =
                    query.Where(c =>
                        c.IsActive);
            }
            else if (status == "inactive")
            {
                query =
                    query.Where(c =>
                        !c.IsActive);
            }


            var categories =
                await query
                    .OrderBy(c =>
                        c.DisplayOrder)
                    .ThenBy(c =>
                        c.Name)
                    .ToListAsync();


            var categoryIds =
                categories
                    .Select(c =>
                        c.Id)
                    .ToList();


            var courseCounts =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        categoryIds.Contains(
                            c.CategoryId))
                    .GroupBy(c =>
                        c.CategoryId)
                    .Select(g =>
                        new
                        {
                            CategoryId =
                                g.Key,

                            Count =
                                g.Count()
                        })
                    .ToDictionaryAsync(
                        x =>
                            x.CategoryId,
                        x =>
                            x.Count);


            var model =
                new AdminCategoriesPageViewModel
                {
                    Search =
                        search,

                    Status =
                        status,

                    TotalCategories =
                        await _context.Categories
                            .AsNoTracking()
                            .CountAsync(),

                    ActiveCategories =
                        await _context.Categories
                            .AsNoTracking()
                            .CountAsync(c =>
                                c.IsActive),

                    InactiveCategories =
                        await _context.Categories
                            .AsNoTracking()
                            .CountAsync(c =>
                                !c.IsActive),

                    TotalCourses =
                        await _context.Courses
                            .AsNoTracking()
                            .CountAsync()
                };


            foreach (var category in categories)
            {
                model.Categories.Add(
                    new AdminCategoryItemViewModel
                    {
                        Id =
                            category.Id,

                        Name =
                            category.Name,

                        Slug =
                        category.Slug
                        ?? string.Empty,

                        Description =
                            category.Description,

                        Icon =
                            category.Icon,

                        DisplayOrder =
                            category.DisplayOrder,

                        IsActive =
                            category.IsActive,

                        CourseCount =
                            courseCounts.TryGetValue(
                                category.Id,
                                out var count)
                                ? count
                                : 0
                    });
            }


            return View(model);
        }


        // =====================================================
        // TẠO DANH MỤC - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int nextOrder =
                await GetNextDisplayOrderAsync();


            var model =
                new AdminCategoryFormViewModel
                {
                    DisplayOrder =
                        nextOrder,

                    IsActive =
                        true
                };


            return View(model);
        }


        // =====================================================
        // TẠO DANH MỤC - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminCategoryFormViewModel model)
        {
            // Slug được phép để trống.
            // ASP.NET có thể đã thêm lỗi Required vào ModelState
            // trước khi action chạy, nên cần xóa lỗi đó trước.
            ModelState.Remove(
                nameof(model.Slug));


            NormalizeFormModel(
                model);


            if (string.IsNullOrWhiteSpace(
                model.Slug))
            {
                model.Slug =
                    GenerateSlug(
                        model.Name);
            }


            await ValidateUniqueAsync(
                model,
                null);


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var category =
                new Category
                {
                    Name =
                        model.Name,

                    Slug =
                        model.Slug,

                    Description =
                        CleanNullable(
                            model.Description),

                    Icon =
                        CleanNullable(
                            model.Icon),

                    DisplayOrder =
                        model.DisplayOrder,

                    IsActive =
                        model.IsActive
                };


            _context.Categories.Add(
                category);


            await _context.SaveChangesAsync();


            await ReorderCategoryAsync(
                category.Id,
                model.DisplayOrder);


            TempData["AdminCategoriesSuccess"] =
                $"Đã tạo danh mục \"{category.Name}\".";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // SỬA DANH MỤC - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var category =
                await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (category == null)
            {
                return NotFound();
            }


            var model =
                new AdminCategoryFormViewModel
                {
                    Id =
                        category.Id,

                    Name =
                        category.Name,

                    Slug =
                        category.Slug
                        ?? string.Empty,

                    Description =
                        category.Description,

                    Icon =
                        category.Icon,

                    DisplayOrder =
                        category.DisplayOrder,

                    IsActive =
                        category.IsActive
                };


            return View(model);
        }


        // =====================================================
        // SỬA DANH MỤC - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AdminCategoryFormViewModel model)
        {
            // Slug được phép để trống.
            ModelState.Remove(
                nameof(model.Slug));


            NormalizeFormModel(
                model);


            if (!model.Id.HasValue)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                model.Slug))
            {
                model.Slug =
                    GenerateSlug(
                        model.Name);
            }


            await ValidateUniqueAsync(
                model,
                model.Id.Value);


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var category =
                await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.Id ==
                        model.Id.Value);


            if (category == null)
            {
                return NotFound();
            }


            category.Name =
                model.Name;

            category.Slug =
                model.Slug;

            category.Description =
                CleanNullable(
                    model.Description);

            category.Icon =
                CleanNullable(
                    model.Icon);

            category.DisplayOrder =
                model.DisplayOrder;

            category.IsActive =
                model.IsActive;


            await _context.SaveChangesAsync();


            await ReorderCategoryAsync(
                category.Id,
                model.DisplayOrder);


            TempData["AdminCategoriesSuccess"] =
                $"Đã cập nhật danh mục \"{category.Name}\".";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // ACTIVE / INACTIVE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            int id,
            string? returnSearch,
            string? returnStatus)
        {
            var category =
                await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (category == null)
            {
                return NotFound();
            }


            category.IsActive =
                !category.IsActive;


            await _context.SaveChangesAsync();


            TempData["AdminCategoriesSuccess"] =
                category.IsActive
                    ? $"Đã kích hoạt danh mục \"{category.Name}\"."
                    : $"Đã tạm ẩn danh mục \"{category.Name}\".";


            return RedirectToIndex(
                returnSearch,
                returnStatus);
        }


        // =====================================================
        // XÓA DANH MỤC AN TOÀN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? returnSearch,
            string? returnStatus)
        {
            var category =
                await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (category == null)
            {
                return NotFound();
            }


            bool hasCourses =
                await _context.Courses
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.CategoryId ==
                        id);


            if (hasCourses)
            {
                TempData["AdminCategoriesError"] =
                    "Không thể xóa danh mục vì đang có khóa học sử dụng danh mục này. Hãy chuyển danh mục sang trạng thái tạm ẩn thay vì xóa.";

                return RedirectToIndex(
                    returnSearch,
                    returnStatus);
            }


            string name =
                category.Name;


            try
            {
                _context.Categories.Remove(
                    category);

                await _context.SaveChangesAsync();

                await NormalizeDisplayOrderAsync();


                TempData["AdminCategoriesSuccess"] =
                    $"Đã xóa danh mục \"{name}\".";
            }
            catch (DbUpdateException)
            {
                TempData["AdminCategoriesError"] =
                    "Không thể xóa danh mục vì vẫn còn dữ liệu liên quan. Hãy tạm ẩn danh mục để giữ an toàn dữ liệu.";
            }


            return RedirectToIndex(
                returnSearch,
                returnStatus);
        }


        // =====================================================
        // VALIDATION
        // =====================================================

        private async Task ValidateUniqueAsync(
            AdminCategoryFormViewModel model,
            int? currentId)
        {
            bool duplicateName =
                await _context.Categories
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.Name ==
                        model.Name &&
                        (
                            !currentId.HasValue ||
                            c.Id !=
                            currentId.Value
                        ));


            if (duplicateName)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Tên danh mục đã tồn tại.");
            }


            bool duplicateSlug =
                await _context.Categories
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.Slug ==
                        model.Slug &&
                        (
                            !currentId.HasValue ||
                            c.Id !=
                            currentId.Value
                        ));


            if (duplicateSlug)
            {
                ModelState.AddModelError(
                    nameof(model.Slug),
                    "Slug đã tồn tại. Vui lòng dùng slug khác.");
            }
        }


        // =====================================================
        // DISPLAY ORDER
        // =====================================================

        private async Task<int>
            GetNextDisplayOrderAsync()
        {
            int maxOrder =
                await _context.Categories
                    .AsNoTracking()
                    .Select(c =>
                        (int?)c.DisplayOrder)
                    .MaxAsync()
                    ?? 0;


            return maxOrder + 1;
        }


        private async Task ReorderCategoryAsync(
            int categoryId,
            int desiredOrder)
        {
            var categories =
                await _context.Categories
                    .OrderBy(c =>
                        c.DisplayOrder)
                    .ThenBy(c =>
                        c.Id)
                    .ToListAsync();


            var target =
                categories.FirstOrDefault(c =>
                    c.Id ==
                    categoryId);


            if (target == null)
            {
                return;
            }


            categories.Remove(
                target);


            int targetIndex =
                Math.Clamp(
                    desiredOrder - 1,
                    0,
                    categories.Count);


            categories.Insert(
                targetIndex,
                target);


            for (int index = 0;
                 index < categories.Count;
                 index++)
            {
                categories[index]
                    .DisplayOrder =
                    index + 1;
            }


            await _context.SaveChangesAsync();
        }


        private async Task NormalizeDisplayOrderAsync()
        {
            var categories =
                await _context.Categories
                    .OrderBy(c =>
                        c.DisplayOrder)
                    .ThenBy(c =>
                        c.Id)
                    .ToListAsync();


            for (int index = 0;
                 index < categories.Count;
                 index++)
            {
                categories[index]
                    .DisplayOrder =
                    index + 1;
            }


            await _context.SaveChangesAsync();
        }


        // =====================================================
        // HELPER
        // =====================================================

        private IActionResult RedirectToIndex(
            string? search,
            string? status)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    search,
                    status
                });
        }


        private static string?
            NormalizeStatus(
                string? status)
        {
            return status switch
            {
                "active" =>
                    "active",

                "inactive" =>
                    "inactive",

                _ =>
                    null
            };
        }


        private static void NormalizeFormModel(
            AdminCategoryFormViewModel model)
        {
            model.Name =
                model.Name?.Trim()
                ?? string.Empty;

            model.Slug =
                model.Slug?.Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            model.Description =
                CleanNullable(
                    model.Description);

            model.Icon =
                CleanNullable(
                    model.Icon);

            if (model.DisplayOrder < 1)
            {
                model.DisplayOrder =
                    1;
            }
        }


        private static string?
            CleanNullable(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }


        private static string GenerateSlug(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return string.Empty;
            }


            string normalized =
                value.Trim()
                    .ToLowerInvariant()
                    .Replace("đ", "d")
                    .Replace("Đ", "d")
                    .Normalize(
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
                    builder.Append(
                        character);
                }
            }


            string slug =
                builder
                    .ToString()
                    .Normalize(
                        NormalizationForm.FormC);


            slug =
                Regex.Replace(
                    slug,
                    @"[^a-z0-9]+",
                    "-");


            return slug.Trim('-');
        }
    }
}
