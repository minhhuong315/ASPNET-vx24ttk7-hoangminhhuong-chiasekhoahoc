using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [AllowAnonymous]
    public class ExploreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ExploreController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // DANH SÁCH GIẢNG VIÊN
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Instructors()
        {
            var instructorUsers =
                await _userManager
                    .GetUsersInRoleAsync("Instructor");


            var activeInstructors =
                instructorUsers
                    .Where(u => u.IsActive)
                    .OrderBy(u =>
                        string.IsNullOrWhiteSpace(u.FullName)
                            ? u.Email
                            : u.FullName)
                    .ToList();


            var instructorIds =
                activeInstructors
                    .Select(u => u.Id)
                    .ToList();


            var courseStats =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.IsPublished &&
                        instructorIds.Contains(
                            c.InstructorId))
                    .GroupBy(c =>
                        c.InstructorId)
                    .Select(g =>
                        new
                        {
                            InstructorId =
                                g.Key,

                            PublishedCourseCount =
                                g.Count(),

                            StudentCount =
                                g.Sum(c =>
                                    c.EnrollmentCount)
                        })
                    .ToDictionaryAsync(
                        x => x.InstructorId);


            var model =
                new InstructorsPageViewModel
                {
                    Instructors =
                        activeInstructors
                            .Select(user =>
                            {
                                courseStats.TryGetValue(
                                    user.Id,
                                    out var stats);


                                return new InstructorPublicItemViewModel
                                {
                                    Id =
                                        user.Id,

                                    FullName =
                                        !string.IsNullOrWhiteSpace(
                                            user.FullName)
                                            ? user.FullName
                                            : user.Email
                                                ?? "Giảng viên EduLearn",

                                    Bio =
                                        user.Bio,

                                    PublishedCourseCount =
                                        stats?.PublishedCourseCount
                                        ?? 0,

                                    StudentCount =
                                        stats?.StudentCount
                                        ?? 0
                                };
                            })
                            .ToList()
                };


            return View(model);
        }


        // =========================================
        // VỀ EDULEARN
        // =========================================

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }
    }
}
