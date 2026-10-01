using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(
            ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
            = string.Empty;
    }


    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;


        [Required]
        public string Token { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}
