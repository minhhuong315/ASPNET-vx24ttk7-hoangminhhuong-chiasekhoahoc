using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminCoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // DANH SÁCH + LỌC KHÓA HỌC
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            int? categoryId)
        {
            search =
                search?.Trim();

            status =
                NormalizeStatus(
                    status);


            var query =
                _context.Courses
                    .AsNoTracking()
                    .Include(c =>
                        c.Category)
                    .Include(c =>
                        c.Instructor)
                    .AsQueryable();


            if (!string.IsNullOrWhiteSpace(
                search))
            {
                query =
                    query.Where(c =>
                        c.Title.Contains(search) ||
                        (
                            c.Instructor.FullName != null &&
                            c.Instructor.FullName.Contains(search)
                        ));
            }


            if (status == "published")
            {
                query =
                    query.Where(c =>
                        c.IsPublished);
            }
            else if (status == "draft")
            {
                query =
                    query.Where(c =>
                        !c.IsPublished);
            }


            if (categoryId.HasValue)
            {
                query =
                    query.Where(c =>
                        c.CategoryId ==
                        categoryId.Value);
            }


            var courses =
                await query
                    .OrderByDescending(c =>
                        c.UpdatedAt)
                    .ThenBy(c =>
                        c.Title)
                    .Select(c =>
                        new AdminCourseItemViewModel
                        {
                            Id =
                                c.Id,

                            Title =
                                c.Title,

                            Slug =
                                c.Slug,

                            CategoryName =
                                c.Category.Name,

                            InstructorName =
                                c.Instructor.FullName,

                            Price =
                                c.Price,

                            DiscountPrice =
                                c.DiscountPrice,

                            IsPublished =
                                c.IsPublished,

                            IsFeatured =
                                c.IsFeatured,

                            CreatedAt =
                                c.CreatedAt,

                            UpdatedAt =
                                c.UpdatedAt,

                            EnrollmentCount =
                                c.Enrollments.Count(),

                            ModuleCount =
                                c.Modules.Count(),

                            LessonCount =
                                c.Modules
                                    .SelectMany(m =>
                                        m.Lessons)
                                    .Count(),

                            ReviewCount =
                                c.Reviews.Count()
                        })
                    .ToListAsync();


            var model =
                new AdminCoursesPageViewModel
                {
                    Search =
                        search,

                    Status =
                        status,

                    CategoryId =
                        categoryId,

                    TotalCourses =
                        await _context.Courses
                            .AsNoTracking()
                            .CountAsync(),

                    PublishedCourses =
                        await _context.Courses
                            .AsNoTracking()
                            .CountAsync(c =>
                                c.IsPublished),

                    DraftCourses =
                        await _context.Courses
                            .AsNoTracking()
                            .CountAsync(c =>
                                !c.IsPublished),

                    TotalEnrollments =
                        await _context.Enrollments
                            .AsNoTracking()
                            .CountAsync(),

                    Courses =
                        courses,

                    Categories =
                        await _context.Categories
                            .AsNoTracking()
                            .Where(c =>
                                c.IsActive)
                            .OrderBy(c =>
                                c.DisplayOrder)
                            .ThenBy(c =>
                                c.Name)
                            .Select(c =>
                                new AdminCourseCategoryOptionViewModel
                                {
                                    Id =
                                        c.Id,

                                    Name =
                                        c.Name
                                })
                            .ToListAsync()
                };


            return View(model);
        }


        // =====================================================
        // PUBLISH / UNPUBLISH
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(
            int id,
            string? returnSearch,
            string? returnStatus,
            int? returnCategoryId)
        {
            var course =
                await _context.Courses
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (course == null)
            {
                return NotFound();
            }


            if (!course.IsPublished)
            {
                bool hasModule =
                    await _context.Modules
                        .AsNoTracking()
                        .AnyAsync(m =>
                            m.CourseId ==
                            course.Id);


                bool hasLesson =
                    await _context.Lessons
                        .AsNoTracking()
                        .AnyAsync(l =>
                            l.Module.CourseId ==
                            course.Id);


                if (!hasModule ||
                    !hasLesson)
                {
                    TempData["AdminCoursesError"] =
                        "Không thể xuất bản: khóa học phải có ít nhất 1 chương và 1 bài học.";

                    return RedirectToIndex(
                        returnSearch,
                        returnStatus,
                        returnCategoryId);
                }


                bool isFirstPublish =
                    !course.PublishedAt.HasValue;


                course.IsPublished =
                    true;

                course.PublishedAt ??=
                    DateTime.UtcNow;


                if (isFirstPublish)
                {
                    await AddNewCourseNotificationsAsync(
                        course);
                }


                TempData["AdminCoursesSuccess"] =
                    $"Đã xuất bản khóa học \"{course.Title}\".";
            }
            else
            {
                course.IsPublished =
                    false;

                TempData["AdminCoursesSuccess"] =
                    $"Đã chuyển khóa học \"{course.Title}\" về bản nháp.";
            }


            course.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            return RedirectToIndex(
                returnSearch,
                returnStatus,
                returnCategoryId);
        }


        // =====================================================
        // ĐẶT / BỎ NỔI BẬT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(
            int id,
            string? returnSearch,
            string? returnStatus,
            int? returnCategoryId)
        {
            var course =
                await _context.Courses
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (course == null)
            {
                return NotFound();
            }


            if (!course.IsPublished)
            {
                TempData["AdminCoursesError"] =
                    "Chỉ khóa học đã xuất bản mới có thể đặt làm nổi bật.";

                return RedirectToIndex(
                    returnSearch,
                    returnStatus,
                    returnCategoryId);
            }


            course.IsFeatured =
                !course.IsFeatured;

            course.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            TempData["AdminCoursesSuccess"] =
                course.IsFeatured
                    ? $"Đã đặt \"{course.Title}\" làm khóa học nổi bật."
                    : $"Đã bỏ \"{course.Title}\" khỏi danh sách nổi bật.";


            return RedirectToIndex(
                returnSearch,
                returnStatus,
                returnCategoryId);
        }


        // =====================================================
        // XÓA KHÓA HỌC AN TOÀN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? returnSearch,
            string? returnStatus,
            int? returnCategoryId)
        {
            var course =
                await _context.Courses
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (course == null)
            {
                return NotFound();
            }


            if (course.IsPublished)
            {
                TempData["AdminCoursesError"] =
                    "Hãy chuyển khóa học về bản nháp trước khi xóa.";

                return RedirectToIndex(
                    returnSearch,
                    returnStatus,
                    returnCategoryId);
            }


            bool hasEnrollments =
                await _context.Enrollments
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.CourseId ==
                        id);


            bool hasReviews =
                await _context.Reviews
                    .AsNoTracking()
                    .AnyAsync(r =>
                        r.CourseId ==
                        id);


            bool hasWishlists =
                await _context.Wishlists
                    .AsNoTracking()
                    .AnyAsync(w =>
                        w.CourseId ==
                        id);


            var lessonIds =
                await _context.Lessons
                    .AsNoTracking()
                    .Where(l =>
                        l.Module.CourseId ==
                        id)
                    .Select(l =>
                        l.Id)
                    .ToListAsync();


            bool hasProgress =
                lessonIds.Any() &&
                await _context.LessonProgresses
                    .AsNoTracking()
                    .AnyAsync(p =>
                        lessonIds.Contains(
                            p.LessonId));


            bool hasQuestions =
                lessonIds.Any() &&
                await _context.LessonQuestions
                    .AsNoTracking()
                    .AnyAsync(q =>
                        lessonIds.Contains(
                            q.LessonId));


            if (hasEnrollments ||
                hasReviews ||
                hasWishlists ||
                hasProgress ||
                hasQuestions)
            {
                TempData["AdminCoursesError"] =
                    "Không thể xóa khóa học vì đã phát sinh dữ liệu học viên, tiến độ, yêu thích, đánh giá hoặc hỏi đáp. Bạn có thể giữ khóa ở trạng thái bản nháp.";

                return RedirectToIndex(
                    returnSearch,
                    returnStatus,
                    returnCategoryId);
            }


            string title =
                course.Title;


            _context.Courses.Remove(
                course);


            await _context.SaveChangesAsync();


            TempData["AdminCoursesSuccess"] =
                $"Đã xóa khóa học \"{title}\".";


            return RedirectToIndex(
                returnSearch,
                returnStatus,
                returnCategoryId);
        }


        // =====================================================
        // THÔNG BÁO KHÓA HỌC MỚI CHO HỌC VIÊN
        // =====================================================

        private async Task AddNewCourseNotificationsAsync(
            Course course)
        {
            var students =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Student");


            var activeStudentIds =
                students
                    .Where(u =>
                        u.IsActive &&
                        u.Id != course.InstructorId)
                    .Select(u =>
                        u.Id)
                    .Where(id =>
                        !string.IsNullOrWhiteSpace(
                            id))
                    .Distinct()
                    .ToList();


            if (!activeStudentIds.Any())
            {
                return;
            }


            var relatedUrl =
                Url.Action(
                    "Details",
                    "Courses",
                    new
                    {
                        slug =
                            course.Slug
                    })
                ?? "/Courses";


            var createdAt =
                DateTime.UtcNow;


            var notifications =
                activeStudentIds
                    .Select(studentId =>
                        new Notification
                        {
                            UserId =
                                studentId,

                            Title =
                                "Khóa học mới trên EduLearn",

                            Message =
                                $"Khóa học \"{course.Title}\" vừa được xuất bản. Khám phá nội dung và bắt đầu học ngay.",

                            Type =
                                "NewCourse",

                            RelatedUrl =
                                relatedUrl,

                            IsRead =
                                false,

                            ReadAt =
                                null,

                            CreatedAt =
                                createdAt
                        })
                    .ToList();


            _context.Notifications.AddRange(
                notifications);
        }


        // =====================================================
        // HELPER
        // =====================================================

        private IActionResult RedirectToIndex(
            string? search,
            string? status,
            int? categoryId)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    search,
                    status,
                    categoryId
                });
        }


        private static string?
            NormalizeStatus(
                string? status)
        {
            return status switch
            {
                "published" =>
                    "published",

                "draft" =>
                    "draft",

                _ =>
                    null
            };
        }
    }
}
