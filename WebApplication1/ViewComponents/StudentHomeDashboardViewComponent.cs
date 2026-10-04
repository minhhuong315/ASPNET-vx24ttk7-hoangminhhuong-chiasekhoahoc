using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.ViewComponents
{
    public class StudentHomeDashboardViewComponent
        : ViewComponent
    {
        private readonly ApplicationDbContext
            _context;

        private readonly UserManager<ApplicationUser>
            _userManager;


        public StudentHomeDashboardViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context =
                context;

            _userManager =
                userManager;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId =
                _userManager.GetUserId(
                    HttpContext.User);


            if (string.IsNullOrWhiteSpace(
                userId))
            {
                return Content(
                    string.Empty);
            }


            var enrollments =
                await _context.Enrollments
                    .AsNoTracking()
                    .Where(e =>
                        e.UserId == userId &&
                        e.Course.IsPublished)
                    .Include(e =>
                        e.Course)
                        .ThenInclude(c =>
                            c.Category)
                    .OrderByDescending(e =>
                        e.LastAccessedAt)
                    .ThenByDescending(e =>
                        e.EnrolledAt)
                    .ToListAsync();


            var enrolledCourseIds =
                enrollments
                    .Select(e =>
                        e.CourseId)
                    .ToList();


            var wishlistCount =
                await _context.Wishlists
                    .AsNoTracking()
                    .CountAsync(w =>
                        w.UserId == userId);


            var openQuestionCount =
                await _context.LessonQuestions
                    .AsNoTracking()
                    .CountAsync(q =>
                        q.UserId == userId &&
                        !q.IsResolved);


            var exploreCourses =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.IsPublished &&
                        !enrolledCourseIds.Contains(
                            c.Id))
                    .Include(c =>
                        c.Category)
                    .OrderByDescending(c =>
                        c.IsFeatured)
                    .ThenByDescending(c =>
                        c.EnrollmentCount)
                    .ThenByDescending(c =>
                        c.PublishedAt)
                    .Take(2)
                    .Select(c =>
                        new StudentHomeExploreCourseViewModel
                        {
                            CourseId =
                                c.Id,

                            Title =
                                c.Title,

                            Slug =
                                c.Slug,

                            CategoryName =
                                c.Category.Name,

                            CategoryIcon =
                                c.Category.Icon
                                ?? "IT",

                            ThumbnailUrl =
                                c.ThumbnailUrl,

                            Level =
                                c.Level,

                            EnrollmentCount =
                                c.EnrollmentCount
                        })
                    .ToListAsync();


            decimal averageProgress =
                enrollments.Count > 0
                    ? Math.Round(
                        enrollments.Average(e =>
                            e.Progress),
                        0,
                        MidpointRounding
                            .AwayFromZero)
                    : 0m;


            var model =
                new StudentHomeDashboardViewModel
                {
                    RegisteredCourseCount =
                        enrollments.Count,

                    InProgressCourseCount =
                        enrollments.Count(e =>
                            e.Progress < 100),

                    CompletedCourseCount =
                        enrollments.Count(e =>
                            e.Progress >= 100),

                    WishlistCount =
                        wishlistCount,

                    OpenQuestionCount =
                        openQuestionCount,

                    AverageProgress =
                        averageProgress,

                    RecentCourses =
                        enrollments
                            .Take(4)
                            .Select(e =>
                                new StudentHomeCourseItemViewModel
                                {
                                    CourseId =
                                        e.CourseId,

                                    Title =
                                        e.Course.Title,

                                    Slug =
                                        e.Course.Slug,

                                    CategoryName =
                                        e.Course.Category.Name,

                                    CategoryIcon =
                                        e.Course.Category.Icon
                                        ?? "IT",

                                    ThumbnailUrl =
                                        e.Course.ThumbnailUrl,

                                    Progress =
                                        e.Progress,

                                    EnrolledAt =
                                        e.EnrolledAt,

                                    LastAccessedAt =
                                        e.LastAccessedAt
                                })
                            .ToList(),

                    ExploreCourses =
                        exploreCourses
                };


            return View(model);
        }
    }
}
