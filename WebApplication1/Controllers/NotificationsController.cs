using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public NotificationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // DANH SÁCH THÔNG BÁO
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var notifications =
                await _context.Notifications
                    .AsNoTracking()
                    .Where(n =>
                        n.UserId == userId)
                    .OrderByDescending(n =>
                        n.CreatedAt)
                    .ToListAsync();


            return View(notifications);
        }


        // =========================================
        // MỞ THÔNG BÁO
        //
        // 1. Kiểm tra notification thuộc user.
        // 2. Đánh dấu đã đọc.
        // 3. Xác định đúng trang đích.
        // 4. Redirect.
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Open(
            int id)
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var notification =
                await _context.Notifications
                    .FirstOrDefaultAsync(n =>
                        n.Id == id &&
                        n.UserId == userId);


            if (notification == null)
            {
                return NotFound();
            }


            // =====================================
            // ĐÁNH DẤU ĐÃ ĐỌC
            // =====================================

            if (!notification.IsRead)
            {
                notification.IsRead =
                    true;

                notification.ReadAt =
                    DateTime.UtcNow;


                await _context.SaveChangesAsync();
            }


            // =====================================
            // XÁC ĐỊNH URL ĐÚNG
            // =====================================

            var destinationUrl =
                await ResolveDestinationUrlAsync(
                    notification,
                    userId);


            if (!string.IsNullOrWhiteSpace(
                    destinationUrl) &&
                Url.IsLocalUrl(
                    destinationUrl))
            {
                return LocalRedirect(
                    destinationUrl);
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // ĐÁNH DẤU TẤT CẢ ĐÃ ĐỌC
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }


            var unreadNotifications =
                await _context.Notifications
                    .Where(n =>
                        n.UserId == userId &&
                        !n.IsRead)
                    .ToListAsync();


            if (unreadNotifications.Any())
            {
                var now =
                    DateTime.UtcNow;


                foreach (var notification
                    in unreadNotifications)
                {
                    notification.IsRead =
                        true;

                    notification.ReadAt =
                        now;
                }


                await _context.SaveChangesAsync();
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // XÁC ĐỊNH ĐÍCH ĐẾN
        //
        // Có xử lý luôn các notification CŨ
        // đã lưu RelatedUrl không còn phù hợp.
        // =========================================

        private async Task<string?>
            ResolveDestinationUrlAsync(
                Notification notification,
                string userId)
        {
            // =====================================
            // STUDENT:
            // GIẢNG VIÊN ĐÃ TRẢ LỜI
            // =====================================

            if (string.Equals(
                notification.Type,
                "QuestionAnswered",
                StringComparison.OrdinalIgnoreCase))
            {
                /*
                 * Notification mới:
                 * nếu RelatedUrl đã trỏ đúng
                 * MyQuestions thì dùng luôn.
                 */

                if (!string.IsNullOrWhiteSpace(
                        notification.RelatedUrl) &&
                    Url.IsLocalUrl(
                        notification.RelatedUrl) &&
                    notification.RelatedUrl.Contains(
                        "/LessonQuestions/MyQuestions",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return notification.RelatedUrl;
                }


                /*
                 * Notification cũ:
                 *
                 * Trước đây nó có thể đang trỏ
                 * sang Courses/Learn.
                 *
                 * Ta tìm phản hồi được tạo gần
                 * thời điểm notification nhất.
                 */

                var fromTime =
                    notification.CreatedAt
                        .AddMinutes(-2);


                var toTime =
                    notification.CreatedAt
                        .AddSeconds(15);


                var questionId =
                    await _context.LessonAnswers
                        .Where(a =>
                            a.Question.UserId ==
                                userId &&
                            a.CreatedAt >=
                                fromTime &&
                            a.CreatedAt <=
                                toTime)
                        .OrderByDescending(a =>
                            a.CreatedAt)
                        .Select(a =>
                            (int?)a.QuestionId)
                        .FirstOrDefaultAsync();


                if (questionId.HasValue)
                {
                    return Url.Action(
                        "MyQuestions",
                        "LessonQuestions",
                        new
                        {
                            selectedQuestionId =
                                questionId.Value
                        });
                }


                /*
                 * Nếu notification quá cũ
                 * không truy ra được chính xác,
                 * vẫn đưa về Câu hỏi của tôi,
                 * KHÔNG đưa về bài học nữa.
                 */

                return Url.Action(
                    "MyQuestions",
                    "LessonQuestions");
            }


            // =====================================
            // INSTRUCTOR:
            // HỌC VIÊN ĐẶT CÂU HỎI MỚI
            // =====================================

            if (string.Equals(
                notification.Type,
                "NewQuestion",
                StringComparison.OrdinalIgnoreCase))
            {
                /*
                 * Notification mới:
                 * URL đã chứa selectedQuestionId.
                 */

                if (!string.IsNullOrWhiteSpace(
                        notification.RelatedUrl) &&
                    Url.IsLocalUrl(
                        notification.RelatedUrl) &&
                    notification.RelatedUrl.Contains(
                        "/LessonQuestions",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return notification.RelatedUrl;
                }


                /*
                 * Khôi phục notification cũ.
                 */

                var fromTime =
                    notification.CreatedAt
                        .AddMinutes(-2);


                var toTime =
                    notification.CreatedAt
                        .AddSeconds(15);


                var questionId =
                    await _context.LessonQuestions
                        .Where(q =>
                            q.Lesson
                                .Module
                                .Course
                                .InstructorId ==
                                userId &&
                            q.CreatedAt >=
                                fromTime &&
                            q.CreatedAt <=
                                toTime)
                        .OrderByDescending(q =>
                            q.CreatedAt)
                        .Select(q =>
                            (int?)q.Id)
                        .FirstOrDefaultAsync();


                if (questionId.HasValue)
                {
                    return Url.Action(
                        "Index",
                        "LessonQuestions",
                        new
                        {
                            selectedQuestionId =
                                questionId.Value
                        });
                }


                return Url.Action(
                    "Index",
                    "LessonQuestions");
            }


            // =====================================
            // CÁC LOẠI THÔNG BÁO KHÁC
            // =====================================

            if (!string.IsNullOrWhiteSpace(
                    notification.RelatedUrl) &&
                Url.IsLocalUrl(
                    notification.RelatedUrl))
            {
                return notification.RelatedUrl;
            }


            return Url.Action(
                nameof(Index),
                "Notifications");
        }
    }
}