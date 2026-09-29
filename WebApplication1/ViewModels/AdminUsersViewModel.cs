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
            Users
        { get; set; }
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
}
