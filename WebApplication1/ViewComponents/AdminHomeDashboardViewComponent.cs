using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.ViewComponents
{
    public class AdminHomeDashboardViewComponent
        : ViewComponent
    {
        private readonly ApplicationDbContext
            _context;

        private readonly UserManager<ApplicationUser>
            _userManager;


        public AdminHomeDashboardViewComponent(
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
            var students =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Student");

            var instructors =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Instructor");


            var totalUsers =
                await _context.Users
                    .AsNoTracking()
                    .CountAsync();


            var totalCourses =
                await _context.Courses
                    .AsNoTracking()
                    .CountAsync();


            var publishedCourses =
                await _context.Courses
                    .AsNoTracking()
                    .CountAsync(c =>
                        c.IsPublished);


            var totalEnrollments =
                await _context.Enrollments
                    .AsNoTracking()
                    .CountAsync();


            var activeCategories =
                await _context.Categories
                    .AsNoTracking()
                    .CountAsync(c =>
                        c.IsActive);


            var pendingQuestions =
                await _context.LessonQuestions
                    .AsNoTracking()
                    .CountAsync(q =>
                        !q.IsResolved);


            var recentCourses =
                await _context.Courses
                    .AsNoTracking()
                    .Include(c =>
                        c.Instructor)
                    .Include(c =>
                        c.Category)
                    .Include(c =>
                        c.Enrollments)
                    .OrderByDescending(c =>
                        c.UpdatedAt)
                    .ThenBy(c =>
                        c.Title)
                    .Take(5)
                    .Select(c =>
                        new AdminHomeCourseItemViewModel
                        {
                            CourseId =
                                c.Id,

                            Title =
                                c.Title,

                            InstructorName =
                                string.IsNullOrWhiteSpace(
                                    c.Instructor.FullName)
                                    ? "Giảng viên"
                                    : c.Instructor.FullName,

                            CategoryName =
                                c.Category.Name,

                            IsPublished =
                                c.IsPublished,

                            EnrollmentCount =
                                c.Enrollments.Count,

                            UpdatedAt =
                                c.UpdatedAt
                        })
                    .ToListAsync();


            var model =
                new AdminHomeDashboardViewModel
                {
                    TotalUsers =
                        totalUsers,

                    StudentCount =
                        students.Count(u =>
                            u.IsActive),

                    InstructorCount =
                        instructors.Count(u =>
                            u.IsActive),

                    TotalCourses =
                        totalCourses,

                    PublishedCourses =
                        publishedCourses,

                    TotalEnrollments =
                        totalEnrollments,

                    DraftCourses =
                        totalCourses -
                        publishedCourses,

                    ActiveCategories =
                        activeCategories,

                    PendingQuestions =
                        pendingQuestions,

                    RecentCourses =
                        recentCourses
                };


            return View(model);
        }
    }
}
