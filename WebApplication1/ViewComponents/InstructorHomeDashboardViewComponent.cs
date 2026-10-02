using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.ViewComponents
{
    public class InstructorHomeDashboardViewComponent
        : ViewComponent
    {
        private readonly ApplicationDbContext
            _context;

        private readonly UserManager<ApplicationUser>
            _userManager;


        public InstructorHomeDashboardViewComponent(
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
                    .Include(c =>
                        c.Enrollments)
                    .OrderByDescending(c =>
                        c.UpdatedAt)
                    .ThenBy(c =>
                        c.Title)
                    .ToListAsync();


            var uniqueStudents =
                await _context.Enrollments
                    .AsNoTracking()
                    .Where(e =>
                        e.Course.InstructorId ==
                        userId)
                    .Select(e =>
                        e.UserId)
                    .Distinct()
                    .CountAsync();


            var pendingQuestions =
                await _context.LessonQuestions
                    .AsNoTracking()
                    .CountAsync(q =>
                        q.Lesson
                            .Module
                            .Course
                            .InstructorId ==
                            userId &&
                        !q.IsResolved);


            var averageRating =
                await _context.Reviews
                    .AsNoTracking()
                    .Where(r =>
                        r.IsApproved &&
                        r.Course.InstructorId ==
                        userId)
                    .Select(r =>
                        (double?)r.Rating)
                    .AverageAsync()
                ?? 0;


            var model =
                new InstructorHomeDashboardViewModel
                {
                    TotalCourses =
                        courses.Count,

                    PublishedCourses =
                        courses.Count(c =>
                            c.IsPublished),

                    DraftCourses =
                        courses.Count(c =>
                            !c.IsPublished),

                    UniqueStudents =
                        uniqueStudents,

                    PendingQuestions =
                        pendingQuestions,

                    AverageRating =
                        averageRating,

                    TotalLessons =
                        courses.Sum(c =>
                            c.Modules.Sum(m =>
                                m.Lessons.Count)),

                    RecentCourses =
                        courses
                            .Take(5)
                            .Select(c =>
                                new InstructorHomeCourseItemViewModel
                                {
                                    CourseId =
                                        c.Id,

                                    Title =
                                        c.Title,

                                    CategoryName =
                                        c.Category?.Name
                                        ?? "Chưa phân loại",

                                    IsPublished =
                                        c.IsPublished,

                                    LessonCount =
                                        c.Modules.Sum(m =>
                                            m.Lessons.Count),

                                    EnrollmentCount =
                                        c.Enrollments.Count,

                                    UpdatedAt =
                                        c.UpdatedAt
                                })
                            .ToList()
                };


            return View(model);
        }
    }
}
