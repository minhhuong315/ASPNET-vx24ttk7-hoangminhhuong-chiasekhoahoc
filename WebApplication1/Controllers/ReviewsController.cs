using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ReviewsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // TẠO / CẬP NHẬT ĐÁNH GIÁ
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            int courseId,
            int rating,
            string? comment)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var course =
                await _context.Courses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == courseId &&
                        c.IsPublished);


            if (course == null)
            {
                return NotFound();
            }


            // Chỉ học viên đã đăng ký khóa học
            // mới được đánh giá.
            bool isEnrolled =
                await _context.Enrollments
                    .AnyAsync(e =>
                        e.UserId == userId &&
                        e.CourseId == courseId);


            if (!isEnrolled)
            {
                TempData["ReviewError"] =
                    "Bạn cần đăng ký khóa học trước khi gửi đánh giá.";


                return RedirectToReviews(
                    course.Slug);
            }


            if (rating < 1 ||
                rating > 5)
            {
                TempData["ReviewError"] =
                    "Số sao phải từ 1 đến 5.";


                return RedirectToReviews(
                    course.Slug);
            }


            comment =
                CleanNullable(
                    comment);


            if (comment != null &&
                comment.Length > 1000)
            {
                TempData["ReviewError"] =
                    "Nội dung đánh giá không được vượt quá 1000 ký tự.";


                return RedirectToReviews(
                    course.Slug);
            }


            var review =
                await _context.Reviews
                    .FirstOrDefaultAsync(r =>
                        r.UserId == userId &&
                        r.CourseId == courseId);


            if (review == null)
            {
                review =
                    new Review
                    {
                        UserId =
                            userId,

                        CourseId =
                            courseId,

                        Rating =
                            rating,

                        Comment =
                            comment,

                        CreatedAt =
                            DateTime.UtcNow,

                        UpdatedAt =
                            DateTime.UtcNow,

                        IsApproved =
                            true
                    };


                _context.Reviews.Add(
                    review);


                TempData["ReviewSuccess"] =
                    "Đánh giá của bạn đã được gửi thành công.";
            }
            else
            {
                review.Rating =
                    rating;

                review.Comment =
                    comment;

                review.UpdatedAt =
                    DateTime.UtcNow;


                TempData["ReviewSuccess"] =
                    "Đánh giá của bạn đã được cập nhật.";
            }


            await _context.SaveChangesAsync();


            return RedirectToReviews(
                course.Slug);
        }


        // =========================================
        // XÓA ĐÁNH GIÁ
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
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
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == courseId &&
                        c.IsPublished);


            if (course == null)
            {
                return NotFound();
            }


            var review =
                await _context.Reviews
                    .FirstOrDefaultAsync(r =>
                        r.UserId == userId &&
                        r.CourseId == courseId);


            if (review != null)
            {
                _context.Reviews.Remove(
                    review);


                await _context.SaveChangesAsync();


                TempData["ReviewSuccess"] =
                    "Đánh giá của bạn đã được xóa.";
            }
            else
            {
                TempData["ReviewInfo"] =
                    "Bạn chưa có đánh giá cho khóa học này.";
            }


            return RedirectToReviews(
                course.Slug);
        }


        // =========================================
        // REDIRECT VỀ KHU VỰC REVIEW
        // =========================================

        private IActionResult RedirectToReviews(
            string slug)
        {
            var url =
                Url.Action(
                    "Details",
                    "Courses",
                    new
                    {
                        slug
                    })
                ?? "/Courses";


            return Redirect(
                $"{url}#reviews");
        }


        // =========================================
        // CLEAN STRING
        // =========================================

        private static string? CleanNullable(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
