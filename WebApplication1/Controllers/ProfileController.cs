using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        // =========================================
        // THÔNG TIN TÀI KHOẢN
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            var model =
                await BuildProfileViewModelAsync(
                    user);


            return View(model);
        }


        // =========================================
        // CẬP NHẬT HỒ SƠ
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            AccountProfileViewModel model)
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            if (!ModelState.IsValid)
            {
                await FillReadOnlyDataAsync(
                    model,
                    user);


                return View(
                    "Index",
                    model);
            }


            user.FullName =
                model.FullName.Trim();

            user.PhoneNumber =
                CleanNullable(
                    model.PhoneNumber);

            user.Bio =
                CleanNullable(
                    model.Bio);

            user.UpdatedAt =
                DateTime.UtcNow;


            var result =
                await _userManager.UpdateAsync(
                    user);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }


                await FillReadOnlyDataAsync(
                    model,
                    user);


                return View(
                    "Index",
                    model);
            }


            await _signInManager.RefreshSignInAsync(
                user);


            TempData["ProfileSuccess"] =
                "Thông tin tài khoản đã được cập nhật.";


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // ĐỔI MẬT KHẨU - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            bool hasPassword =
                await _userManager.HasPasswordAsync(
                    user);


            if (!hasPassword)
            {
                TempData["ProfileError"] =
                    "Tài khoản hiện chưa sử dụng mật khẩu cục bộ.";


                return RedirectToAction(
                    nameof(Index));
            }


            return View(
                new ChangePasswordViewModel());
        }


        // =========================================
        // ĐỔI MẬT KHẨU - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        TranslateIdentityError(
                            error.Code,
                            error.Description));
                }


                return View(model);
            }


            user.UpdatedAt =
                DateTime.UtcNow;


            await _userManager.UpdateAsync(
                user);


            await _signInManager.RefreshSignInAsync(
                user);


            TempData["ProfileSuccess"] =
                "Mật khẩu đã được thay đổi thành công.";


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // BUILD VIEW MODEL
        // =========================================

        private async Task<AccountProfileViewModel>
            BuildProfileViewModelAsync(
                ApplicationUser user)
        {
            var model =
                new AccountProfileViewModel
                {
                    FullName =
                        user.FullName,

                    PhoneNumber =
                        user.PhoneNumber,

                    Bio =
                        user.Bio
                };


            await FillReadOnlyDataAsync(
                model,
                user);


            return model;
        }


        // =========================================
        // DỮ LIỆU CHỈ ĐỌC
        // =========================================

        private async Task FillReadOnlyDataAsync(
            AccountProfileViewModel model,
            ApplicationUser user)
        {
            model.Email =
                user.Email
                ?? string.Empty;

            model.UserName =
                user.UserName
                ?? string.Empty;

            model.CreatedAt =
                user.CreatedAt;

            model.UpdatedAt =
                user.UpdatedAt;

            model.EmailConfirmed =
                user.EmailConfirmed;

            model.IsActive =
                user.IsActive;


            var roles =
                await _userManager
                    .GetRolesAsync(user);


            model.Role =
                roles.FirstOrDefault()
                ?? "User";
        }


        // =========================================
        // CLEAN NULLABLE
        // =========================================

        private static string? CleanNullable(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        // =========================================
        // THÔNG BÁO IDENTITY DỄ HIỂU HƠN
        // =========================================

        private static string TranslateIdentityError(
            string code,
            string description)
        {
            return code switch
            {
                "PasswordMismatch" =>
                    "Mật khẩu hiện tại không đúng.",

                "PasswordTooShort" =>
                    "Mật khẩu mới chưa đủ độ dài.",

                "PasswordRequiresNonAlphanumeric" =>
                    "Mật khẩu mới cần có ít nhất một ký tự đặc biệt.",

                "PasswordRequiresDigit" =>
                    "Mật khẩu mới cần có ít nhất một chữ số.",

                "PasswordRequiresUpper" =>
                    "Mật khẩu mới cần có ít nhất một chữ hoa.",

                "PasswordRequiresLower" =>
                    "Mật khẩu mới cần có ít nhất một chữ thường.",

                _ =>
                    description
            };
        }
    }
}
