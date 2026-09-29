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
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // DASHBOARD QUẢN TRỊ
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var students =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Student");


            var instructors =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Instructor");


            var admins =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Admin");


            var totalUsers =
                await _userManager.Users
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


            var totalReviews =
                await _context.Reviews
                    .AsNoTracking()
                    .CountAsync();


            var recentCourses =
                await _context.Courses
                    .AsNoTracking()
                    .Include(c =>
                        c.Category)
                    .Include(c =>
                        c.Instructor)
                    .OrderByDescending(c =>
                        c.UpdatedAt)
                    .ThenByDescending(c =>
                        c.CreatedAt)
                    .Take(6)
                    .Select(c =>
                        new AdminRecentCourseViewModel
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

                            IsPublished =
                                c.IsPublished,

                            EnrollmentCount =
                                c.Enrollments.Count(),

                            UpdatedAt =
                                c.UpdatedAt
                        })
                    .ToListAsync();


            var model =
                new AdminDashboardViewModel
                {
                    TotalUsers =
                        totalUsers,

                    TotalStudents =
                        students.Count,

                    TotalInstructors =
                        instructors.Count,

                    TotalCourses =
                        totalCourses,

                    PublishedCourses =
                        publishedCourses,

                    DraftCourses =
                        totalCourses -
                        publishedCourses,

                    TotalEnrollments =
                        totalEnrollments,

                    TotalReviews =
                        totalReviews,

                    RecentCourses =
                        recentCourses
                };


            return View(model);
        }
    }
}
