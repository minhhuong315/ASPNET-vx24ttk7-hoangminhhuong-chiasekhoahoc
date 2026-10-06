using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Services;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // =========================
        // ĐĂNG KÝ
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = model.FullName.Trim(),
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (result.Succeeded)
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Student");

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    ModelState.AddModelError(
                        string.Empty,
                        "Không thể gán quyền học viên.");

                    return View(model);
                }

                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                TempData["SuccessMessage"] =
                    "Đăng ký tài khoản EduLearn thành công!";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }


        // =========================
        // ĐĂNG NHẬP
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email hoặc mật khẩu không chính xác.");

                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản của bạn đã bị khóa.");

                return View(model);
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrWhiteSpace(returnUrl)
                    && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Email hoặc mật khẩu không chính xác.");

            return View(model);
        }


        // =========================
        // QUÊN MẬT KHẨU
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email =
                model.Email.Trim();

            var user =
                await _userManager.FindByEmailAsync(
                    email);

            // Không tiết lộ email có tồn tại trong hệ thống hay không.
            if (user == null ||
                !user.IsActive)
            {
                return RedirectToAction(
                    nameof(ForgotPasswordConfirmation));
            }

            var token =
                await _userManager
                    .GeneratePasswordResetTokenAsync(
                        user);

            var encodedToken =
                WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(
                        token));

            var resetUrl =
                Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new
                    {
                        email =
                            user.Email,

                        token =
                            encodedToken
                    },
                    Request.Scheme);

            if (string.IsNullOrWhiteSpace(
                resetUrl))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể tạo liên kết đặt lại mật khẩu.");

                return View(model);
            }

            var safeName =
                WebUtility.HtmlEncode(
                    string.IsNullOrWhiteSpace(
                        user.FullName)
                        ? "bạn"
                        : user.FullName);

            var safeResetUrl =
                WebUtility.HtmlEncode(
                    resetUrl);

            var htmlMessage =
                $"""
                <div style="font-family:Arial,Helvetica,sans-serif;line-height:1.7;color:#273142;">
                    <h2 style="margin:0 0 14px;color:#0d6efd;">
                        EduLearn
                    </h2>

                    <p>
                        Xin chào {safeName},
                    </p>

                    <p>
                        EduLearn nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                    </p>

                    <p style="margin:24px 0;">
                        <a href="{safeResetUrl}"
                           style="display:inline-block;padding:12px 18px;background:#0d6efd;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:700;">
                            Đặt lại mật khẩu
                        </a>
                    </p>

                    <p>
                        Liên kết đặt lại mật khẩu có hiệu lực trong 2 giờ.
                        Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.
                    </p>

                    <p style="margin-top:28px;color:#667085;">
                        EduLearn - Chia sẻ tri thức, kiến tạo tương lai
                    </p>
                </div>
                """;

            try
            {
                await _emailService.SendAsync(
                    user.Email ?? email,
                    "EduLearn - Khôi phục mật khẩu",
                    htmlMessage);
            }
            catch
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể gửi email khôi phục lúc này. Vui lòng kiểm tra cấu hình email hoặc thử lại sau.");

                return View(model);
            }

            return RedirectToAction(
                nameof(ForgotPasswordConfirmation));
        }


        // =========================
        // XÁC NHẬN KHÔI PHỤC
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        // =========================
        // ĐẶT LẠI MẬT KHẨU - GET
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(
            string? email,
            string? token)
        {
            if (string.IsNullOrWhiteSpace(
                    email) ||
                string.IsNullOrWhiteSpace(
                    token))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            var model =
                new ResetPasswordViewModel
                {
                    Email =
                        email,

                    Token =
                        token
                };

            return View(model);
        }


        // =========================
        // ĐẶT LẠI MẬT KHẨU - POST
        // =========================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email =
                model.Email.Trim();

            var user =
                await _userManager.FindByEmailAsync(
                    email);

            if (user == null ||
                !user.IsActive)
            {
                return RedirectToAction(
                    nameof(ResetPasswordConfirmation));
            }

            string decodedToken;

            try
            {
                decodedToken =
                    Encoding.UTF8.GetString(
                        WebEncoders.Base64UrlDecode(
                            model.Token));
            }
            catch
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Liên kết đặt lại mật khẩu không hợp lệ.");

                return View(model);
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    decodedToken,
                    model.NewPassword);

            if (result.Succeeded)
            {
                user.UpdatedAt =
                    DateTime.UtcNow;

                await _userManager.UpdateAsync(
                    user);

                return RedirectToAction(
                    nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }


        // =========================
        // XÁC NHẬN ĐẶT LẠI THÀNH CÔNG
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        // =========================
        // KHÔNG CÓ QUYỀN TRUY CẬP
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return View();
        }



        // =========================
        // ĐĂNG XUẤT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}
