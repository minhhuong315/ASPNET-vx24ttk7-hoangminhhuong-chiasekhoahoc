using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.ViewComponents
{
    public class HomePublicDataViewComponent
        : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public HomePublicDataViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IViewComponentResult>
            InvokeAsync()
        {
            // =====================================
            // THỐNG KÊ CÔNG KHAI
            // =====================================

            int publishedCourseCount =
                await _context.Courses
                    .AsNoTracking()
                    .CountAsync(c =>
                        c.IsPublished);


            var students =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Student");


            var instructors =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Instructor");


            int studentCount =
                students.Count(u =>
                    u.IsActive);


            int instructorCount =
                instructors.Count(u =>
                    u.IsActive);


            double averageRating =
                await _context.Reviews
                    .AsNoTracking()
                    .Where(r =>
                        r.IsApproved &&
                        r.Course.IsPublished)
                    .Select(r =>
                        (double?)r.Rating)
                    .AverageAsync()
                ?? 0d;


            // =====================================
            // DANH MỤC ĐANG HOẠT ĐỘNG
            // =====================================

            var categories =
                await _context.Categories
                    .AsNoTracking()
                    .Where(c =>
                        c.IsActive)
                    .OrderBy(c =>
                        c.DisplayOrder)
                    .ThenBy(c =>
                        c.Name)
                    .Take(8)
                    .Select(c =>
                        new HomeCategoryItemViewModel
                        {
                            Id =
                                c.Id,

                            Name =
                                c.Name,

                            Icon =
                                string.IsNullOrWhiteSpace(
                                    c.Icon)
                                    ? "IT"
                                    : c.Icon
                        })
                    .ToListAsync();


            var categoryIds =
                categories
                    .Select(c =>
                        c.Id)
                    .ToList();


            var categoryCourseCounts =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.IsPublished &&
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


            foreach (var category in categories)
            {
                category.PublishedCourseCount =
                    categoryCourseCounts.TryGetValue(
                        category.Id,
                        out var count)
                        ? count
                        : 0;
            }


            // =====================================
            // KHÓA HỌC NỔI BẬT / ĐƯỢC QUAN TÂM
            // =====================================

            var featuredCourses =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.IsPublished)
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
                    .Take(4)
                    .Select(c =>
                        new HomeFeaturedCourseViewModel
                        {
                            Id =
                                c.Id,

                            Title =
                                c.Title,

                            Slug =
                                c.Slug,

                            CategoryName =
                                c.Category.Name,

                            CategoryIcon =
                                string.IsNullOrWhiteSpace(
                                    c.Category.Icon)
                                    ? "IT"
                                    : c.Category.Icon,

                            InstructorName =
                                string.IsNullOrWhiteSpace(
                                    c.Instructor.FullName)
                                    ? "Giảng viên EduLearn"
                                    : c.Instructor.FullName,

                            Level =
                                c.Level,

                            EnrollmentCount =
                                c.Enrollments.Count(),

                            IsFeatured =
                                c.IsFeatured
                        })
                    .ToListAsync();


            var model =
                new HomePublicDataViewModel
                {
                    PublishedCourseCount =
                        publishedCourseCount,

                    StudentCount =
                        studentCount,

                    InstructorCount =
                        instructorCount,

                    AverageRating =
                        Math.Round(
                            averageRating,
                            1),

                    Categories =
                        categories,

                    FeaturedCourses =
                        featuredCourses
                };


            return View(model);
        }
    }
}
