using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels
{
    public class AdminUsersPageViewModel
    {
        public string? Search { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }


        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int InactiveUsers { get; set; }

        public int TotalStudents { get; set; }

        public int TotalInstructors { get; set; }


        public List<AdminUserItemViewModel>
            Users { get; set; }
                = new List<AdminUserItemViewModel>();
    }


    public class AdminUserItemViewModel
    {
        public string Id { get; set; }
            = string.Empty;

        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Role { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsCurrentUser { get; set; }
    }


    public class AdminCreateUserViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(
            100,
            ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(
            ErrorMessage = "Email không hợp lệ.")]
        [StringLength(
            256,
            ErrorMessage = "Email không được vượt quá 256 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng chọn vai trò.")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; }
            = "Student";
    }
}
