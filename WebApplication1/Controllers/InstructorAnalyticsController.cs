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
    public class InstructorAnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public InstructorAnalyticsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // TỔNG QUAN PHÂN TÍCH GIẢNG VIÊN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instructorId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(
                instructorId))
            {
                return Challenge();
            }


            var courses =
                await _context.Courses
                    .AsNoTracking()
                    .Where(c =>
                        c.InstructorId ==
                        instructorId)
                    .OrderByDescending(c =>
                        c.UpdatedAt)
                    .ThenBy(c =>
                        c.Title)
                    .Select(c =>
                        new
                        {
                            c.Id,
                            c.Title,
                            c.Slug,
                            c.IsPublished,
                            c.IsFeatured,
                            c.UpdatedAt
                        })
                    .ToListAsync();


            var model =
                new InstructorAnalyticsViewModel
                {
                    TotalCourses =
                        courses.Count,

                    PublishedCourses =
                        courses.Count(c =>
                            c.IsPublished),

                    DraftCourses =
                        courses.Count(c =>
                            !c.IsPublished)
                };


            if (!courses.Any())
            {
                return View(model);
            }


            var courseIds =
                courses
                    .Select(c =>
                        c.Id)
                    .ToList();


            var lessonCounts =
                await _context.Lessons
                    .AsNoTracking()
                    .Where(l =>
                        courseIds.Contains(
                            l.Module.CourseId))
                    .GroupBy(l =>
                        l.Module.CourseId)
                    .Select(g =>
                        new
                        {
                            CourseId =
                                g.Key,

                            Count =
                                g.Count()
                        })
                    .ToDictionaryAsync(
                        x =>
                            x.CourseId,
                        x =>
                            x.Count);


            var enrollments =
                await _context.Enrollments
                    .AsNoTracking()
                    .Where(e =>
                        courseIds.Contains(
                            e.CourseId))
                    .Select(e =>
                        new
                        {
                            e.CourseId,
                            e.UserId,
                            e.EnrolledAt,
                            e.LastAccessedAt
                        })
                    .ToListAsync();


            model.TotalEnrollments =
                enrollments.Count;

            model.UniqueStudents =
                enrollments
                    .Select(e =>
                        e.UserId)
                    .Distinct()
                    .Count();


            var completedProgress =
                await _context.LessonProgresses
                    .AsNoTracking()
                    .Where(p =>
                        p.IsCompleted &&
                        courseIds.Contains(
                            p.Lesson.Module.CourseId))
                    .GroupBy(p =>
                        new
                        {
                            CourseId =
                                p.Lesson.Module.CourseId,

                            p.UserId
                        })
                    .Select(g =>
                        new
                        {
                            g.Key.CourseId,
                            g.Key.UserId,
                            CompletedLessons =
                                g.Count()
                        })
                    .ToListAsync();


            var completedLookup =
                completedProgress
                    .ToDictionary(
                        x =>
                            (
                                x.CourseId,
                                x.UserId
                            ),
                        x =>
                            x.CompletedLessons);


            var reviewStats =
                await _context.Reviews
                    .AsNoTracking()
                    .Where(r =>
                        courseIds.Contains(
                            r.CourseId) &&
                        r.IsApproved)
                    .GroupBy(r =>
                        r.CourseId)
                    .Select(g =>
                        new
                        {
                            CourseId =
                                g.Key,

                            ReviewCount =
                                g.Count(),

                            AverageRating =
                                g.Average(r =>
                                    r.Rating)
                        })
                    .ToDictionaryAsync(
                        x =>
                            x.CourseId,
                        x =>
                            x);


            foreach (var course in courses)
            {
                int totalLessons =
                    lessonCounts.TryGetValue(
                        course.Id,
                        out var lessonCount)
                        ? lessonCount
                        : 0;


                var courseEnrollments =
                    enrollments
                        .Where(e =>
                            e.CourseId ==
                            course.Id)
                        .ToList();


                var progressValues =
                    new List<decimal>();


                int completedStudents =
                    0;


                foreach (var enrollment in courseEnrollments)
                {
                    int completedLessons =
                        completedLookup.TryGetValue(
                            (
                                course.Id,
                                enrollment.UserId
                            ),
                            out var completedCount)
                            ? completedCount
                            : 0;


                    decimal progress =
                        totalLessons > 0
                            ? Math.Round(
                                completedLessons *
                                100m /
                                totalLessons,
                                1)
                            : 0m;


                    progress =
                        Math.Clamp(
                            progress,
                            0m,
                            100m);


                    progressValues.Add(
                        progress);


                    if (progress >= 100m)
                    {
                        completedStudents++;
                    }
                }


                decimal averageProgress =
                    progressValues.Any()
                        ? Math.Round(
                            progressValues
                                .Average(),
                            1)
                        : 0m;


                int reviewCount =
                    0;

                double averageRating =
                    0d;


                if (reviewStats.TryGetValue(
                    course.Id,
                    out var courseReviewStat))
                {
                    reviewCount =
                        courseReviewStat
                            .ReviewCount;

                    averageRating =
                        Math.Round(
                            courseReviewStat
                                .AverageRating,
                            1);
                }


                model.Courses.Add(
                    new InstructorCourseAnalyticsItemViewModel
                    {
                        Id =
                            course.Id,

                        Title =
                            course.Title,

                        Slug =
                            course.Slug,

                        IsPublished =
                            course.IsPublished,

                        IsFeatured =
                            course.IsFeatured,

                        UpdatedAt =
                            course.UpdatedAt,

                        TotalLessons =
                            totalLessons,

                        EnrollmentCount =
                            courseEnrollments.Count,

                        CompletedStudents =
                            completedStudents,

                        AverageProgress =
                            averageProgress,

                        ReviewCount =
                            reviewCount,

                        AverageRating =
                            averageRating
                    });
            }


            var weightedProgressTotal =
                model.Courses.Sum(c =>
                    c.AverageProgress *
                    c.EnrollmentCount);


            model.AverageProgress =
                model.TotalEnrollments > 0
                    ? Math.Round(
                        weightedProgressTotal /
                        model.TotalEnrollments,
                        1)
                    : 0m;


            model.CompletedEnrollments =
                model.Courses.Sum(c =>
                    c.CompletedStudents);


            model.CompletionRate =
                model.TotalEnrollments > 0
                    ? Math.Round(
                        model.CompletedEnrollments *
                        100m /
                        model.TotalEnrollments,
                        1)
                    : 0m;


            model.TotalReviews =
                model.Courses.Sum(c =>
                    c.ReviewCount);


            if (model.TotalReviews > 0)
            {
                double weightedRating =
                    model.Courses.Sum(c =>
                        c.AverageRating *
                        c.ReviewCount);


                model.AverageRating =
                    Math.Round(
                        weightedRating /
                        model.TotalReviews,
                        1);
            }


            return View(model);
        }
    }
}
