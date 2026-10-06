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

        // =====================================================
        // DANH SÁCH KHÓA HỌC
        // =====================================================

        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? level,
            string? priceRange,
            decimal? minPrice,
            decimal? maxPrice,
            int? minRating,
            string? sort,
            int page = 1)
        {
            search =
                search?.Trim();

            string? normalizedLevel =
                null;

            if (!string.IsNullOrWhiteSpace(
                level))
            {
                switch (level
                    .Trim()
                    .ToLowerInvariant())
                {
                    case "beginner":
                        normalizedLevel =
                            "Beginner";
                        break;

                    case "intermediate":
                        normalizedLevel =
                            "Intermediate";
                        break;

                    case "advanced":
                        normalizedLevel =
                            "Advanced";
                        break;
                }
            }

            priceRange =
                priceRange?
                    .Trim()
                    .ToLowerInvariant();

            priceRange =
                priceRange switch
                {
                    "free" => "free",
                    "under-1m" => "under-1m",
                    "1m-2m" => "1m-2m",
                    "over-2m" => "over-2m",
                    _ => null
                };

            minPrice =
                minPrice.HasValue &&
                minPrice.Value >= 0
                    ? minPrice.Value
                    : null;

            maxPrice =
                maxPrice.HasValue &&
                maxPrice.Value >= 0
                    ? maxPrice.Value
                    : null;

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                (minPrice, maxPrice) =
                    (maxPrice, minPrice);
            }

            minRating =
                minRating.HasValue &&
                minRating.Value >= 1 &&
                minRating.Value <= 5
                    ? minRating.Value
                    : null;

            sort =
                sort?
                    .Trim()
                    .ToLowerInvariant();

            sort =
                sort switch
                {
                    "newest" => "newest",
                    "price-asc" => "price-asc",
                    "price-desc" => "price-desc",
                    "rating" => "rating",
                    _ => "popular"
                };

            var coursesQuery =
                _context.Courses
                    .AsNoTracking()
                    .Include(c => c.Category)
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Include(c => c.Reviews
                        .Where(r =>
                            r.IsApproved))
                    .Where(c =>
                        c.IsPublished)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                coursesQuery =
                    coursesQuery.Where(c =>
                        c.Title.Contains(search) ||
                        (
                            c.ShortDescription != null &&
                            c.ShortDescription.Contains(search)
                        ) ||
                        (
                            c.Description != null &&
                            c.Description.Contains(search)
                        ) ||
                        c.Category.Name.Contains(search));
            }

            if (categoryId.HasValue)
            {
                coursesQuery =
                    coursesQuery.Where(c =>
                        c.CategoryId ==
                        categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(
                normalizedLevel))
            {
                coursesQuery =
                    coursesQuery.Where(c =>
                        c.Level ==
                        normalizedLevel);
            }

            if (!string.IsNullOrWhiteSpace(
                priceRange))
            {
                coursesQuery =
                    priceRange switch
                    {
                        "free" =>
                            coursesQuery.Where(c =>
                                (
                                    c.DiscountPrice.HasValue &&
                                    c.DiscountPrice.Value < c.Price
                                        ? c.DiscountPrice.Value
                                        : c.Price
                                ) == 0),

                        "under-1m" =>
                            coursesQuery.Where(c =>
                                (
                                    c.DiscountPrice.HasValue &&
                                    c.DiscountPrice.Value < c.Price
                                        ? c.DiscountPrice.Value
                                        : c.Price
                                ) > 0 &&
                                (
                                    c.DiscountPrice.HasValue &&
                                    c.DiscountPrice.Value < c.Price
                                        ? c.DiscountPrice.Value
                                        : c.Price
                                ) < 1000000),

                        "1m-2m" =>
                            coursesQuery.Where(c =>
                                (
                                    c.DiscountPrice.HasValue &&
                                    c.DiscountPrice.Value < c.Price
                                        ? c.DiscountPrice.Value
                                        : c.Price
                                ) >= 1000000 &&
                                (
                                    c.DiscountPrice.HasValue &&
                                    c.DiscountPrice.Value < c.Price
                                        ? c.DiscountPrice.Value
                                        : c.Price
                                ) <= 2000000),

                        "over-2m" =>
                            coursesQuery.Where(c =>
                                (
                                    c.DiscountPrice.HasValue &&
                                    c.DiscountPrice.Value < c.Price
                                        ? c.DiscountPrice.Value
                                        : c.Price
                                ) > 2000000),

                        _ =>
                            coursesQuery
                    };
            }
            else
            {
                // Giữ tương thích với các URL/bộ lọc giá cũ.
                if (minPrice.HasValue)
                {
                    coursesQuery =
                        coursesQuery.Where(c =>
                            (
                                c.DiscountPrice.HasValue &&
                                c.DiscountPrice.Value < c.Price
                                    ? c.DiscountPrice.Value
                                    : c.Price
                            ) >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    coursesQuery =
                        coursesQuery.Where(c =>
                            (
                                c.DiscountPrice.HasValue &&
                                c.DiscountPrice.Value < c.Price
                                    ? c.DiscountPrice.Value
                                    : c.Price
                            ) <= maxPrice.Value);
                }
            }

            if (minRating.HasValue)
            {
                coursesQuery =
                    coursesQuery.Where(c =>
                        c.Reviews.Any(r =>
                            r.IsApproved) &&
                        c.Reviews
                            .Where(r =>
                                r.IsApproved)
                            .Average(r =>
                                (double)r.Rating) >=
                        minRating.Value);
            }

            const int pageSize =
                9;

            if (page < 1)
            {
                page = 1;
            }

            var totalItems =
                await coursesQuery
                    .CountAsync();

            var totalPages =
                totalItems == 0
                    ? 0
                    : (int)Math.Ceiling(
                        totalItems /
                        (double)pageSize);

            if (totalPages == 0)
            {
                page = 1;
            }
            else if (page > totalPages)
            {
                page =
                    totalPages;
            }

            coursesQuery =
                sort switch
                {
                    "newest" =>
                        coursesQuery
                            .OrderByDescending(c =>
                                c.PublishedAt)
                            .ThenByDescending(c =>
                                c.CreatedAt)
                            .ThenBy(c =>
                                c.Title),

                    "price-asc" =>
                        coursesQuery
                            .OrderBy(c =>
                                c.DiscountPrice.HasValue &&
                                c.DiscountPrice.Value < c.Price
                                    ? c.DiscountPrice.Value
                                    : c.Price)
                            .ThenBy(c =>
                                c.Title),

                    "price-desc" =>
                        coursesQuery
                            .OrderByDescending(c =>
                                c.DiscountPrice.HasValue &&
                                c.DiscountPrice.Value < c.Price
                                    ? c.DiscountPrice.Value
                                    : c.Price)
                            .ThenBy(c =>
                                c.Title),

                    "rating" =>
                        coursesQuery
                            .OrderByDescending(c =>
                                c.Reviews
                                    .Where(r =>
                                        r.IsApproved)
                                    .Select(r =>
                                        (double?)r.Rating)
                                    .Average() ?? 0)
                            .ThenByDescending(c =>
                                c.Reviews.Count(r =>
                                    r.IsApproved))
                            .ThenBy(c =>
                                c.Title),

                    _ =>
                        coursesQuery
                            .OrderByDescending(c =>
                                c.IsFeatured)
                            .ThenByDescending(c =>
                                c.Enrollments.Count())
                            .ThenByDescending(c =>
                                c.Reviews.Count(r =>
                                    r.IsApproved))
                            .ThenByDescending(c =>
                                c.PublishedAt)
                            .ThenBy(c =>
                                c.Title)
                };

            var courses =
                await coursesQuery
                    .Skip(
                        (page - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();

            var categories =
                await _context.Categories
                    .AsNoTracking()
                    .Where(c =>
                        c.IsActive)
                    .OrderBy(c =>
                        c.DisplayOrder)
                    .ThenBy(c =>
                        c.Name)
                    .ToListAsync();

            ViewBag.Categories =
                categories;

            ViewBag.Search =
                search;

            ViewBag.CategoryId =
                categoryId;

            ViewBag.Level =
                level?
                    .Trim()
                    .ToLowerInvariant();

            ViewBag.NormalizedLevel =
                normalizedLevel;

            ViewBag.PriceRange =
                priceRange;

            ViewBag.MinPrice =
                minPrice;

            ViewBag.MaxPrice =
                maxPrice;

            ViewBag.MinRating =
                minRating;

            ViewBag.Sort =
                sort;

            ViewBag.CurrentPage =
                page;

            ViewBag.TotalPages =
                totalPages;

            ViewBag.TotalItems =
                totalItems;

            ViewBag.PageSize =
                pageSize;

            ViewBag.PageTitle =
                normalizedLevel switch
                {
                    "Beginner" =>
                        "Khóa học cơ bản",

                    "Intermediate" =>
                        "Khóa học trung cấp",

                    "Advanced" =>
                        "Khóa học nâng cao",

                    _ =>
                        "Tất cả khóa học"
                };

            ViewBag.PageDescription =
                normalizedLevel switch
                {
                    "Beginner" =>
                        "Các khóa học dành cho người mới bắt đầu và xây dựng kiến thức nền tảng.",

                    "Intermediate" =>
                        "Các khóa học dành cho người đã có kiến thức nền tảng và muốn phát triển kỹ năng.",

                    "Advanced" =>
                        "Các khóa học chuyên sâu dành cho người muốn nâng cao kiến thức và kỹ năng.",

                    _ =>
                        "Khám phá các khóa học Công nghệ thông tin trên EduLearn."
                };

            return View(
                courses);
        }


        // =====================================================
        // CHI TIẾT KHÓA HỌC
        // =====================================================

        public async Task<IActionResult> Details(
            string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }


            var course =
                await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Instructor)
                    .Include(c => c.Modules
                        .OrderBy(m =>
                            m.DisplayOrder))
                        .ThenInclude(m =>
                            m.Lessons
                                .OrderBy(l =>
                                    l.DisplayOrder))
                    .Include(c => c.Reviews
                        .Where(r =>
                            r.IsApproved))
                        .ThenInclude(r =>
                            r.User)
                    .FirstOrDefaultAsync(c =>
                        c.Slug == slug &&
                        c.IsPublished);


            if (course == null)
            {
                return NotFound();
            }


            ViewBag.IsEnrolled =
                false;

            ViewBag.EnrollmentProgress =
                0m;

            ViewBag.IsWishlisted =
                false;

            ViewBag.CurrentUserReview =
                null;


            if (User.Identity?.IsAuthenticated == true)
            {
                var userId =
                    _userManager.GetUserId(User);


                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var enrollment =
                        await _context.Enrollments
                            .FirstOrDefaultAsync(e =>
                                e.UserId ==
                                userId &&
                                e.CourseId ==
                                course.Id);


                    if (enrollment != null)
                    {
                        var progress =
                            await RecalculateEnrollmentProgressAsync(
                                userId,
                                course.Id,
                                enrollment);


                        ViewBag.IsEnrolled =
                            true;

                        ViewBag.EnrollmentProgress =
                            progress;
                    }


                    if (User.IsInRole("Student"))
                    {
                        ViewBag.IsWishlisted =
                            await _context.Wishlists
                                .AnyAsync(w =>
                                    w.UserId == userId &&
                                    w.CourseId == course.Id);


                        ViewBag.CurrentUserReview =
                            await _context.Reviews
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r =>
                                    r.UserId == userId &&
                                    r.CourseId == course.Id);
                    }
                }
            }


            course.ViewCount++;


            await _context.SaveChangesAsync();


            return View(course);
        }


        // =====================================================
        // ĐĂNG KÝ KHÓA HỌC
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(
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
                    .FirstOrDefaultAsync(c =>
                        c.Id == courseId &&
                        c.IsPublished);


            if (course == null)
            {
                return NotFound();
            }


            var alreadyEnrolled =
                await _context.Enrollments
                    .AnyAsync(e =>
                        e.UserId ==
                        userId &&
                        e.CourseId ==
                        courseId);


            if (alreadyEnrolled)
            {
                TempData["InfoMessage"] =
                    "Bạn đã đăng ký khóa học này.";


                return RedirectToAction(
                    "Details",
                    new
                    {
                        slug =
                            course.Slug
                    });
            }


            var enrollment =
                new Enrollment
                {
                    UserId =
                        userId,

                    CourseId =
                        courseId,

                    EnrolledAt =
                        DateTime.UtcNow,

                    Progress =
                        0,

                    LastAccessedAt =
                        DateTime.UtcNow
                };


            _context.Enrollments.Add(
                enrollment);


            course.EnrollmentCount++;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Đăng ký khóa học thành công!";


            return RedirectToAction(
                "Details",
                new
                {
                    slug =
                        course.Slug
                });
        }


        // =====================================================
        // KHÓA HỌC CỦA TÔI
        // =====================================================

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var enrollments =
                await _context.Enrollments
                    .Where(e =>
                        e.UserId == userId &&
                        e.Course.IsPublished)
                    .Include(e => e.Course)
                        .ThenInclude(c =>
                            c.Category)
                    .Include(e => e.Course)
                        .ThenInclude(c =>
                            c.Instructor)
                    .OrderByDescending(e =>
                        e.LastAccessedAt ??
                        e.EnrolledAt)
                    .ToListAsync();


            foreach (var enrollment in enrollments)
            {
                await RecalculateEnrollmentProgressAsync(
                    userId,
                    enrollment.CourseId,
                    enrollment);
            }


            await _context.SaveChangesAsync();


            return View(enrollments);
        }


        // =====================================================
        // TRANG HỌC
        // =====================================================

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Learn(
            int courseId,
            int? lessonId)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var enrollment =
                await _context.Enrollments
                    .FirstOrDefaultAsync(e =>
                        e.UserId == userId &&
                        e.CourseId == courseId);


            if (enrollment == null)
            {
                return Forbid();
            }


            var course =
                await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Instructor)
                    .Include(c => c.Modules
                        .OrderBy(m =>
                            m.DisplayOrder))
                        .ThenInclude(m =>
                            m.Lessons
                                .OrderBy(l =>
                                    l.DisplayOrder))
                    .FirstOrDefaultAsync(c =>
                        c.Id == courseId &&
                        c.IsPublished);


            if (course == null)
            {
                return NotFound();
            }


            var lessons =
                GetOrderedLessons(
                    course);


            if (!lessons.Any())
            {
                TempData["InfoMessage"] =
                    "Khóa học chưa có bài học.";


                return RedirectToAction(
                    "Details",
                    new
                    {
                        slug =
                            course.Slug
                    });
            }


            var lessonIds =
                lessons
                    .Select(l => l.Id)
                    .ToList();


            var completedLessonIds =
                await _context.LessonProgresses
                    .Where(p =>
                        p.UserId == userId &&
                        lessonIds.Contains(
                            p.LessonId) &&
                        p.IsCompleted)
                    .Select(p =>
                        p.LessonId)
                    .ToListAsync();


            var firstIncompleteIndex =
                lessons.FindIndex(l =>
                    !completedLessonIds.Contains(
                        l.Id));


            var unlockedLessonIds =
                new List<int>(
                    completedLessonIds);


            if (firstIncompleteIndex >= 0)
            {
                var nextAvailableLesson =
                    lessons[
                        firstIncompleteIndex];


                if (!unlockedLessonIds.Contains(
                    nextAvailableLesson.Id))
                {
                    unlockedLessonIds.Add(
                        nextAvailableLesson.Id);
                }
            }


            Lesson currentLesson;


            if (!lessonId.HasValue)
            {
                if (firstIncompleteIndex >= 0)
                {
                    currentLesson =
                        lessons[
                            firstIncompleteIndex];
                }
                else
                {
                    currentLesson =
                        lessons.First();
                }
            }
            else
            {
                var requestedLesson =
                    lessons.FirstOrDefault(l =>
                        l.Id ==
                        lessonId.Value);


                if (requestedLesson == null)
                {
                    return NotFound();
                }


                if (!unlockedLessonIds.Contains(
                    requestedLesson.Id))
                {
                    TempData["LearningInfoMessage"] =
                        "Hãy hoàn thành bài học trước để mở khóa bài này.";


                    if (firstIncompleteIndex >= 0)
                    {
                        currentLesson =
                            lessons[
                                firstIncompleteIndex];
                    }
                    else
                    {
                        currentLesson =
                            lessons.First();
                    }
                }
                else
                {
                    currentLesson =
                        requestedLesson;
                }
            }


            var progress =
                await RecalculateEnrollmentProgressAsync(
                    userId,
                    courseId,
                    enrollment);


            enrollment.LastAccessedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            ViewBag.CurrentLesson =
                currentLesson;

            ViewBag.CompletedLessonIds =
                completedLessonIds;

            ViewBag.UnlockedLessonIds =
                unlockedLessonIds;

            ViewBag.EnrollmentProgress =
                progress;


            return View(course);
        }


        // =====================================================
        // HOÀN THÀNH BÀI HỌC
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(
            int lessonId)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var lesson =
                await _context.Lessons
                    .Include(l => l.Module)
                    .FirstOrDefaultAsync(l =>
                        l.Id == lessonId);


            if (lesson == null)
            {
                return NotFound();
            }


            var courseId =
                lesson.Module.CourseId;


            var enrollment =
                await _context.Enrollments
                    .FirstOrDefaultAsync(e =>
                        e.UserId == userId &&
                        e.CourseId == courseId);


            if (enrollment == null)
            {
                return Forbid();
            }


            var course =
                await _context.Courses
                    .Include(c => c.Modules
                        .OrderBy(m =>
                            m.DisplayOrder))
                        .ThenInclude(m =>
                            m.Lessons
                                .OrderBy(l =>
                                    l.DisplayOrder))
                    .FirstOrDefaultAsync(c =>
                        c.Id ==
                        courseId &&
                        c.IsPublished);


            if (course == null)
            {
                return NotFound();
            }


            var lessons =
                GetOrderedLessons(
                    course);


            var lessonIds =
                lessons
                    .Select(l => l.Id)
                    .ToList();


            var completedLessonIds =
                await _context.LessonProgresses
                    .Where(p =>
                        p.UserId == userId &&
                        lessonIds.Contains(
                            p.LessonId) &&
                        p.IsCompleted)
                    .Select(p =>
                        p.LessonId)
                    .ToListAsync();


            var targetIndex =
                lessons.FindIndex(l =>
                    l.Id == lessonId);


            if (targetIndex < 0)
            {
                return NotFound();
            }


            var firstIncompleteIndex =
                lessons.FindIndex(l =>
                    !completedLessonIds.Contains(
                        l.Id));


            var lessonAlreadyCompleted =
                completedLessonIds.Contains(
                    lessonId);


            var lessonIsCurrentUnlockedLesson =
                firstIncompleteIndex >= 0 &&
                targetIndex ==
                firstIncompleteIndex;


            if (!lessonAlreadyCompleted &&
                !lessonIsCurrentUnlockedLesson)
            {
                TempData["LearningInfoMessage"] =
                    "Bạn chưa thể hoàn thành bài học này.";


                var availableLessonId =
                    firstIncompleteIndex >= 0
                        ? lessons[
                            firstIncompleteIndex]
                            .Id
                        : lessons.First().Id;


                return RedirectToAction(
                    "Learn",
                    new
                    {
                        courseId,
                        lessonId =
                            availableLessonId
                    });
            }


            var lessonProgress =
                await _context.LessonProgresses
                    .FirstOrDefaultAsync(p =>
                        p.UserId == userId &&
                        p.LessonId ==
                        lessonId);


            if (lessonProgress == null)
            {
                lessonProgress =
                    new LessonProgress
                    {
                        UserId =
                            userId,

                        LessonId =
                            lessonId,

                        IsCompleted =
                            true,

                        WatchedDuration =
                            lesson.Duration,

                        CompletedAt =
                            DateTime.UtcNow,

                        LastWatchedAt =
                            DateTime.UtcNow
                    };


                _context.LessonProgresses.Add(
                    lessonProgress);
            }
            else
            {
                lessonProgress.IsCompleted =
                    true;

                lessonProgress.WatchedDuration =
                    lesson.Duration;

                lessonProgress.CompletedAt ??=
                    DateTime.UtcNow;

                lessonProgress.LastWatchedAt =
                    DateTime.UtcNow;
            }


            await _context.SaveChangesAsync();


            var progress =
                await RecalculateEnrollmentProgressAsync(
                    userId,
                    courseId,
                    enrollment);


            enrollment.LastAccessedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            int nextLessonId;


            if (targetIndex + 1 <
                lessons.Count)
            {
                nextLessonId =
                    lessons[
                        targetIndex + 1]
                        .Id;
            }
            else
            {
                nextLessonId =
                    lessonId;
            }


            TempData["LearningSuccessMessage"] =
                progress >= 100
                    ? "Chúc mừng! Bạn đã hoàn thành khóa học."
                    : "Đã hoàn thành bài học. Bài tiếp theo đã được mở khóa.";


            return RedirectToAction(
                "Learn",
                new
                {
                    courseId,
                    lessonId =
                        nextLessonId
                });
        }


        // =====================================================
        // LẤY DANH SÁCH BÀI THEO THỨ TỰ
        // =====================================================

        private static List<Lesson>
            GetOrderedLessons(
                Course course)
        {
            return course.Modules
                .OrderBy(m =>
                    m.DisplayOrder)
                .SelectMany(m =>
                    m.Lessons
                        .OrderBy(l =>
                            l.DisplayOrder))
                .ToList();
        }


        // =====================================================
        // TÍNH LẠI TIẾN ĐỘ
        // =====================================================

        private async Task<decimal>
            RecalculateEnrollmentProgressAsync(
                string userId,
                int courseId,
                Enrollment enrollment)
        {
            var lessonIds =
                await _context.Lessons
                    .Where(l =>
                        l.Module.CourseId ==
                        courseId)
                    .Select(l =>
                        l.Id)
                    .ToListAsync();


            var totalLessons =
                lessonIds.Count;


            var completedLessons =
                0;


            if (totalLessons > 0)
            {
                completedLessons =
                    await _context.LessonProgresses
                        .CountAsync(p =>
                            p.UserId ==
                            userId &&
                            p.IsCompleted &&
                            lessonIds.Contains(
                                p.LessonId));
            }


            decimal progress =
                0m;


            if (totalLessons > 0)
            {
                progress =
                    Math.Round(
                        (decimal)
                        completedLessons /
                        totalLessons *
                        100m,
                        0,
                        MidpointRounding
                            .AwayFromZero);
            }


            enrollment.Progress =
                Math.Clamp(
                    progress,
                    0m,
                    100m);


            if (enrollment.Progress >=
                100m)
            {
                enrollment.CompletedAt ??=
                    DateTime.UtcNow;
            }
            else
            {
                enrollment.CompletedAt =
                    null;
            }


            return enrollment.Progress;
        }
    }
}