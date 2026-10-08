using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineLearningPlatform.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [HttpGet]
        [ActionName("StatusCode")]
        public IActionResult HandleStatusCode(
            int code)
        {
            Response.StatusCode =
                code;

            ViewBag.StatusCode =
                code;

            ViewBag.Title =
                code switch
                {
                    404 =>
                        "Không tìm thấy trang",

                    403 =>
                        "Không có quyền truy cập",

                    401 =>
                        "Bạn cần đăng nhập",

                    _ =>
                        "Đã xảy ra lỗi"
                };

            ViewBag.Message =
                code switch
                {
                    404 =>
                        "Trang bạn đang tìm không tồn tại, đã bị di chuyển hoặc đường dẫn không còn hợp lệ.",

                    403 =>
                        "Tài khoản hiện tại không có quyền truy cập nội dung này.",

                    401 =>
                        "Bạn cần đăng nhập để tiếp tục truy cập nội dung này.",

                    _ =>
                        "Yêu cầu không thể được xử lý. Vui lòng thử lại hoặc quay về trang chủ."
                };

            return View(
                "StatusCode");
        }


        [HttpGet]
        public IActionResult ServerError()
        {
            Response.StatusCode =
                StatusCodes
                    .Status500InternalServerError;

            return View();
        }
    }
}
