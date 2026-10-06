using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize]
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
        // STUDENT - TẠO / CẬP NHẬT ĐÁNH GIÁ
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Student")]
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


            var student =
                await _userManager.GetUserAsync(
                    User);

            var studentDisplayName =
                !string.IsNullOrWhiteSpace(
                    student?.FullName)
                    ? student.FullName
                    : !string.IsNullOrWhiteSpace(
                        student?.Email)
                        ? student.Email
                        : "Học viên";


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


            bool isNewReview =
                review == null;


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


            if (course.InstructorId != userId)
            {
                var courseUrl =
                    Url.Action(
                        "Details",
                        "Courses",
                        new
                        {
                            slug =
                                course.Slug
                        })
                    ?? "/Courses";

                var instructorNotification =
                    new Notification
                    {
                        UserId =
                            course.InstructorId,

                        Title =
                            isNewReview
                                ? "Có đánh giá mới"
                                : "Đánh giá đã được cập nhật",

                        Message =
                            isNewReview
                                ? $"{studentDisplayName} đã đánh giá khóa học \"{course.Title}\"."
                                : $"{studentDisplayName} đã cập nhật đánh giá trong khóa học \"{course.Title}\".",

                        Type =
                            isNewReview
                                ? "NewReview"
                                : "ReviewUpdated",

                        RelatedUrl =
                            $"{courseUrl}#reviews",

                        IsRead =
                            false,

                        ReadAt =
                            null,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                _context.Notifications.Add(
                    instructorNotification);
            }


            await _context.SaveChangesAsync();


            return RedirectToReviews(
                course.Slug);
        }


        // =========================================
        // STUDENT - XÓA ĐÁNH GIÁ
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Student")]
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
        // INSTRUCTOR - PHẢN HỒI ĐÁNH GIÁ
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(
            int reviewId,
            string? reply)
        {
            var instructorId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(
                instructorId))
            {
                return Challenge();
            }


            var review =
                await _context.Reviews
                    .Include(r =>
                        r.Course)
                    .FirstOrDefaultAsync(r =>
                        r.Id == reviewId);


            if (review == null)
            {
                return NotFound();
            }


            if (review.Course.InstructorId !=
                instructorId)
            {
                return Forbid();
            }


            reply =
                CleanNullable(
                    reply);


            if (reply == null)
            {
                TempData["ReviewReplyError"] =
                    "Vui lòng nhập nội dung phản hồi.";

                return RedirectToReviews(
                    review.Course.Slug);
            }


            if (reply.Length > 1000)
            {
                TempData["ReviewReplyError"] =
                    "Phản hồi không được vượt quá 1000 ký tự.";

                return RedirectToReviews(
                    review.Course.Slug);
            }


            bool isNewReply =
                string.IsNullOrWhiteSpace(
                    review.InstructorReply);


            review.InstructorReply =
                reply;

            review.InstructorReplyUpdatedAt =
                DateTime.UtcNow;


            if (review.UserId !=
                instructorId)
            {
                var courseUrl =
                    Url.Action(
                        "Details",
                        "Courses",
                        new
                        {
                            slug =
                                review.Course.Slug
                        })
                    ?? "/Courses";

                var studentNotification =
                    new Notification
                    {
                        UserId =
                            review.UserId,

                        Title =
                            isNewReply
                                ? "Đánh giá đã được phản hồi"
                                : "Phản hồi đánh giá đã được cập nhật",

                        Message =
                            isNewReply
                                ? $"Giảng viên đã phản hồi đánh giá của bạn trong khóa học \"{review.Course.Title}\"."
                                : $"Giảng viên đã cập nhật phản hồi cho đánh giá của bạn trong khóa học \"{review.Course.Title}\".",

                        Type =
                            isNewReply
                                ? "ReviewReplied"
                                : "ReviewReplyUpdated",

                        RelatedUrl =
                            $"{courseUrl}#reviews",

                        IsRead =
                            false,

                        ReadAt =
                            null,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                _context.Notifications.Add(
                    studentNotification);
            }


            await _context.SaveChangesAsync();


            TempData["ReviewReplySuccess"] =
                isNewReply
                    ? "Phản hồi đánh giá đã được gửi."
                    : "Phản hồi đánh giá đã được cập nhật.";


            return RedirectToReviews(
                review.Course.Slug);
        }


        // =========================================
        // INSTRUCTOR - XÓA PHẢN HỒI
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(
            int reviewId)
        {
            var instructorId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(
                instructorId))
            {
                return Challenge();
            }


            var review =
                await _context.Reviews
                    .Include(r =>
                        r.Course)
                    .FirstOrDefaultAsync(r =>
                        r.Id == reviewId);


            if (review == null)
            {
                return NotFound();
            }


            if (review.Course.InstructorId !=
                instructorId)
            {
                return Forbid();
            }


            if (string.IsNullOrWhiteSpace(
                review.InstructorReply))
            {
                TempData["ReviewReplyInfo"] =
                    "Đánh giá này chưa có phản hồi.";

                return RedirectToReviews(
                    review.Course.Slug);
            }


            review.InstructorReply =
                null;

            review.InstructorReplyUpdatedAt =
                null;


            await _context.SaveChangesAsync();


            TempData["ReviewReplySuccess"] =
                "Phản hồi đánh giá đã được xóa.";


            return RedirectToReviews(
                review.Course.Slug);
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
