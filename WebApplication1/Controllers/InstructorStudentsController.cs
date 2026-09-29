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
    public class InstructorStudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public InstructorStudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // DANH SÁCH HỌC VIÊN + TIẾN ĐỘ THEO KHÓA HỌC
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            int? courseId)
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
                        new InstructorStudentCourseOptionViewModel
                        {
                            Id =
                                c.Id,

                            Title =
                                c.Title,

                            IsPublished =
                                c.IsPublished
                        })
                    .ToListAsync();


            var model =
                new InstructorStudentsPageViewModel
                {
                    Courses =
                        courses
                };


            if (!courses.Any())
            {
                return View(model);
            }


            int selectedCourseId =
                courseId.HasValue &&
                courses.Any(c =>
                    c.Id ==
                    courseId.Value)
                    ? courseId.Value
                    : courses.First().Id;


            var selectedCourse =
                courses.First(c =>
                    c.Id ==
                    selectedCourseId);


            model.SelectedCourseId =
                selectedCourseId;

            model.SelectedCourseTitle =
                selectedCourse.Title;

            model.SelectedCourseIsPublished =
                selectedCourse.IsPublished;


            // =====================================
            // DANH SÁCH BÀI HỌC CỦA KHÓA
            // =====================================

            var lessonIds =
                await _context.Lessons
                    .AsNoTracking()
                    .Where(l =>
                        l.Module.CourseId ==
                        selectedCourseId)
                    .Select(l =>
                        l.Id)
                    .ToListAsync();


            model.TotalLessons =
                lessonIds.Count;


            // =====================================
            // ENROLLMENT
            // =====================================

            var enrollments =
                await _context.Enrollments
                    .AsNoTracking()
                    .Where(e =>
                        e.CourseId ==
                        selectedCourseId)
                    .OrderByDescending(e =>
                        e.EnrolledAt)
                    .Select(e =>
                        new
                        {
                            e.UserId,
                            e.EnrolledAt,
                            e.LastAccessedAt
                        })
                    .ToListAsync();


            if (!enrollments.Any())
            {
                return View(model);
            }


            var studentIds =
                enrollments
                    .Select(e =>
                        e.UserId)
                    .Distinct()
                    .ToList();


            // =====================================
            // THÔNG TIN HỌC VIÊN
            // =====================================

            var studentUsers =
                await _userManager.Users
                    .AsNoTracking()
                    .Where(u =>
                        studentIds.Contains(
                            u.Id))
                    .Select(u =>
                        new
                        {
                            u.Id,
                            u.FullName,
                            u.Email
                        })
                    .ToListAsync();


            var studentLookup =
                studentUsers
                    .ToDictionary(
                        u => u.Id,
                        u => u);


            // =====================================
            // SỐ BÀI ĐÃ HOÀN THÀNH
            // =====================================

            Dictionary<string, int>
                completedLessonCountByUser;


            if (lessonIds.Any())
            {
                completedLessonCountByUser =
                    await _context.LessonProgresses
                        .AsNoTracking()
                        .Where(p =>
                            studentIds.Contains(
                                p.UserId) &&
                            lessonIds.Contains(
                                p.LessonId) &&
                            p.IsCompleted)
                        .GroupBy(p =>
                            p.UserId)
                        .Select(g =>
                            new
                            {
                                UserId =
                                    g.Key,

                                Count =
                                    g.Count()
                            })
                        .ToDictionaryAsync(
                            x =>
                                x.UserId,
                            x =>
                                x.Count);
            }
            else
            {
                completedLessonCountByUser =
                    new Dictionary<string, int>();
            }


            // =====================================
            // BUILD DANH SÁCH HỌC VIÊN
            // =====================================

            foreach (var enrollment in enrollments)
            {
                studentLookup.TryGetValue(
                    enrollment.UserId,
                    out var student);


                int completedLessons =
                    completedLessonCountByUser
                        .TryGetValue(
                            enrollment.UserId,
                            out var count)
                            ? count
                            : 0;


                decimal progress =
                    model.TotalLessons > 0
                        ? Math.Round(
                            completedLessons *
                            100m /
                            model.TotalLessons,
                            1)
                        : 0m;


                string status =
                    progress >= 100m
                        ? "Completed"
                        : progress > 0m
                            ? "Learning"
                            : "NotStarted";


                model.Students.Add(
                    new InstructorStudentItemViewModel
                    {
                        UserId =
                            enrollment.UserId,

                        FullName =
                            !string.IsNullOrWhiteSpace(
                                student?.FullName)
                                ? student.FullName
                                : "Học viên EduLearn",

                        Email =
                            student?.Email
                            ?? string.Empty,

                        EnrolledAt =
                            enrollment.EnrolledAt,

                        LastAccessedAt =
                            enrollment.LastAccessedAt,

                        CompletedLessons =
                            completedLessons,

                        TotalLessons =
                            model.TotalLessons,

                        Progress =
                            progress,

                        Status =
                            status
                    });
            }


            // =====================================
            // THỐNG KÊ
            // =====================================

            model.TotalStudents =
                model.Students.Count;

            model.CompletedStudents =
                model.Students.Count(s =>
                    s.Progress >= 100m);

            model.LearningStudents =
                model.Students.Count(s =>
                    s.Progress > 0m &&
                    s.Progress < 100m);

            model.NotStartedStudents =
                model.Students.Count(s =>
                    s.Progress <= 0m);

            model.AverageProgress =
                model.Students.Any()
                    ? Math.Round(
                        model.Students
                            .Average(s =>
                                s.Progress),
                        1)
                    : 0m;


            return View(model);
        }
    }
}
