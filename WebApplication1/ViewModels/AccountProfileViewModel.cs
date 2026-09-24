using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    public class AccountProfileViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(
            100,
            ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
            = string.Empty;


        [Phone(
            ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(
            30,
            ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }


        [StringLength(
            1000,
            ErrorMessage = "Giới thiệu bản thân không được vượt quá 1000 ký tự.")]
        [Display(Name = "Giới thiệu bản thân")]
        public string? Bio { get; set; }


        public string Email { get; set; }
            = string.Empty;

        public string UserName { get; set; }
            = string.Empty;

        public string Role { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsActive { get; set; }
    }


    public class ChangePasswordViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }
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
            ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}
