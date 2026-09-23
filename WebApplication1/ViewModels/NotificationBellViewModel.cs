using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.ViewModels
{
    public class NotificationBellViewModel
    {
        // Số thông báo chưa đọc
        public int UnreadCount { get; set; }


        // Một số thông báo mới nhất
        // để hiển thị trong dropdown
        public List<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}