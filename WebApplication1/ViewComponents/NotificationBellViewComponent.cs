using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.ViewComponents
{
    public class NotificationBellViewComponent
        : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public NotificationBellViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            /*
             * Nếu chưa đăng nhập thì không hiển thị chuông.
             */
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Content(string.Empty);
            }


            var userId =
                _userManager.GetUserId(
                    HttpContext.User);


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Content(string.Empty);
            }


            // =========================
            // ĐẾM THÔNG BÁO CHƯA ĐỌC
            // =========================

            var unreadCount =
                await _context.Notifications
                    .AsNoTracking()
                    .CountAsync(n =>
                        n.UserId == userId &&
                        !n.IsRead);


            // =========================
            // LẤY 5 THÔNG BÁO MỚI NHẤT
            // =========================

            var notifications =
                await _context.Notifications
                    .AsNoTracking()
                    .Where(n =>
                        n.UserId == userId)
                    .OrderByDescending(n =>
                        n.CreatedAt)
                    .Take(5)
                    .ToListAsync();


            var model =
                new NotificationBellViewModel
                {
                    UnreadCount =
                        unreadCount,

                    Notifications =
                        notifications
                };


            return View(model);
        }
    }
}