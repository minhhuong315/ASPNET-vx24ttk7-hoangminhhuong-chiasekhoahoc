using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminUsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // DANH SÁCH NGƯỜI DÙNG
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? role,
            string? status)
        {
            search =
                search?.Trim();

            role =
                NormalizeRoleFilter(
                    role);

            status =
                NormalizeStatusFilter(
                    status);


            var studentUsers =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Student");

            var instructorUsers =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Instructor");

            var adminUsers =
                await _userManager
                    .GetUsersInRoleAsync(
                        "Admin");


            var studentIds =
                studentUsers
                    .Select(u => u.Id)
                    .ToHashSet();

            var instructorIds =
                instructorUsers
                    .Select(u => u.Id)
                    .ToHashSet();

            var adminIds =
                adminUsers
                    .Select(u => u.Id)
                    .ToHashSet();


            var usersQuery =
                _userManager.Users
                    .AsNoTracking()
                    .AsQueryable();


            // =====================================
            // TÌM KIẾM
            // =====================================

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                usersQuery =
                    usersQuery.Where(u =>
                        (
                            u.FullName != null &&
                            u.FullName.Contains(search)
                        ) ||
                        (
                            u.Email != null &&
                            u.Email.Contains(search)
                        ));
            }


            // =====================================
            // TRẠNG THÁI
            // =====================================

            if (status == "active")
            {
                usersQuery =
                    usersQuery.Where(u =>
                        u.IsActive);
            }
            else if (status == "inactive")
            {
                usersQuery =
                    usersQuery.Where(u =>
                        !u.IsActive);
            }


            var users =
                await usersQuery
                    .OrderByDescending(u =>
                        u.CreatedAt)
                    .ThenBy(u =>
                        u.FullName)
                    .ToListAsync();


            // =====================================
            // ROLE FILTER
            // =====================================

            if (role == "Student")
            {
                users =
                    users
                        .Where(u =>
                            studentIds.Contains(
                                u.Id))
                        .ToList();
            }
            else if (role == "Instructor")
            {
                users =
                    users
                        .Where(u =>
                            instructorIds.Contains(
                                u.Id))
                        .ToList();
            }
            else if (role == "Admin")
            {
                users =
                    users
                        .Where(u =>
                            adminIds.Contains(
                                u.Id))
                        .ToList();
            }


            var currentUserId =
                _userManager.GetUserId(User);


            var model =
                new AdminUsersPageViewModel
                {
                    Search =
                        search,

                    Role =
                        role,

                    Status =
                        status,

                    TotalUsers =
                        await _userManager.Users
                            .AsNoTracking()
                            .CountAsync(),

                    ActiveUsers =
                        await _userManager.Users
                            .AsNoTracking()
                            .CountAsync(u =>
                                u.IsActive),

                    InactiveUsers =
                        await _userManager.Users
                            .AsNoTracking()
                            .CountAsync(u =>
                                !u.IsActive),

                    TotalStudents =
                        studentIds.Count,

                    TotalInstructors =
                        instructorIds.Count
                };


            foreach (var user in users)
            {
                string userRole =
                    adminIds.Contains(user.Id)
                        ? "Admin"
                        : instructorIds.Contains(
                            user.Id)
                            ? "Instructor"
                            : studentIds.Contains(
                                user.Id)
                                ? "Student"
                                : "User";


                model.Users.Add(
                    new AdminUserItemViewModel
                    {
                        Id =
                            user.Id,

                        FullName =
                            string.IsNullOrWhiteSpace(
                                user.FullName)
                                ? "Người dùng EduLearn"
                                : user.FullName,

                        Email =
                            user.Email
                            ?? string.Empty,

                        Role =
                            userRole,

                        IsActive =
                            user.IsActive,

                        EmailConfirmed =
                            user.EmailConfirmed,

                        CreatedAt =
                            user.CreatedAt,

                        UpdatedAt =
                            user.UpdatedAt,

                        IsCurrentUser =
                            user.Id ==
                            currentUserId
                    });
            }


            return View(model);
        }


        // =====================================================
        // KHÓA / MỞ KHÓA TÀI KHOẢN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            string id,
            string? returnSearch,
            string? returnRole,
            string? returnStatus)
        {
            var currentUserId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }


            var user =
                await _userManager
                    .FindByIdAsync(id);


            if (user == null)
            {
                return NotFound();
            }


            if (user.Id == currentUserId)
            {
                TempData["AdminUsersError"] =
                    "Bạn không thể khóa chính tài khoản quản trị đang đăng nhập.";

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            if (await _userManager.IsInRoleAsync(
                user,
                "Admin"))
            {
                TempData["AdminUsersError"] =
                    "Tài khoản quản trị được bảo vệ và không thể khóa tại trang này.";

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            bool newActiveStatus =
                !user.IsActive;


            user.IsActive =
                newActiveStatus;

            user.UpdatedAt =
                DateTime.UtcNow;


            var updateResult =
                await _userManager
                    .UpdateAsync(user);


            if (!updateResult.Succeeded)
            {
                TempData["AdminUsersError"] =
                    BuildIdentityErrorMessage(
                        updateResult,
                        "Không thể cập nhật trạng thái tài khoản.");

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            if (newActiveStatus)
            {
                await _userManager
                    .SetLockoutEndDateAsync(
                        user,
                        null);

                await _userManager
                    .ResetAccessFailedCountAsync(
                        user);

                TempData["AdminUsersSuccess"] =
                    $"Đã mở khóa tài khoản {user.Email}.";
            }
            else
            {
                await _userManager
                    .SetLockoutEnabledAsync(
                        user,
                        true);

                await _userManager
                    .SetLockoutEndDateAsync(
                        user,
                        DateTimeOffset.UtcNow
                            .AddYears(100));

                await _userManager
                    .UpdateSecurityStampAsync(
                        user);

                TempData["AdminUsersSuccess"] =
                    $"Đã khóa tài khoản {user.Email}.";
            }


            return RedirectToIndex(
                returnSearch,
                returnRole,
                returnStatus);
        }


        // =====================================================
        // ĐỔI ROLE STUDENT / INSTRUCTOR
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(
            string id,
            string newRole,
            string? returnSearch,
            string? returnRole,
            string? returnStatus)
        {
            var currentUserId =
                _userManager.GetUserId(User);


            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }


            if (newRole != "Student" &&
                newRole != "Instructor")
            {
                TempData["AdminUsersError"] =
                    "Vai trò được chọn không hợp lệ.";

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            var user =
                await _userManager
                    .FindByIdAsync(id);


            if (user == null)
            {
                return NotFound();
            }


            if (user.Id == currentUserId)
            {
                TempData["AdminUsersError"] =
                    "Bạn không thể thay đổi vai trò của chính tài khoản quản trị đang đăng nhập.";

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            var currentRoles =
                await _userManager
                    .GetRolesAsync(user);


            if (currentRoles.Contains(
                "Admin"))
            {
                TempData["AdminUsersError"] =
                    "Vai trò Admin được bảo vệ và không thể thay đổi tại trang này.";

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            string? currentManagedRole =
                currentRoles.Contains(
                    "Instructor")
                    ? "Instructor"
                    : currentRoles.Contains(
                        "Student")
                        ? "Student"
                        : null;


            if (currentManagedRole ==
                newRole)
            {
                TempData["AdminUsersInfo"] =
                    "Tài khoản đã có vai trò này.";

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            // Không cho chuyển Instructor đang sở hữu Course
            // thành Student để tránh dữ liệu Course bị lệch nghiệp vụ.
            if (currentManagedRole ==
                    "Instructor" &&
                newRole ==
                    "Student")
            {
                bool ownsCourses =
                    await _context.Courses
                        .AsNoTracking()
                        .AnyAsync(c =>
                            c.InstructorId ==
                            user.Id);


                if (ownsCourses)
                {
                    TempData["AdminUsersError"] =
                        "Không thể chuyển giảng viên này thành học viên vì tài khoản đang sở hữu khóa học.";

                    return RedirectToIndex(
                        returnSearch,
                        returnRole,
                        returnStatus);
                }
            }


            // Xóa role cũ trước.
            if (!string.IsNullOrWhiteSpace(
                currentManagedRole))
            {
                var removeResult =
                    await _userManager
                        .RemoveFromRoleAsync(
                            user,
                            currentManagedRole);


                if (!removeResult.Succeeded)
                {
                    TempData["AdminUsersError"] =
                        BuildIdentityErrorMessage(
                            removeResult,
                            "Không thể gỡ vai trò hiện tại.");

                    return RedirectToIndex(
                        returnSearch,
                        returnRole,
                        returnStatus);
                }
            }


            var addResult =
                await _userManager
                    .AddToRoleAsync(
                        user,
                        newRole);


            if (!addResult.Succeeded)
            {
                // Khôi phục role cũ nếu thêm role mới thất bại.
                if (!string.IsNullOrWhiteSpace(
                    currentManagedRole))
                {
                    await _userManager
                        .AddToRoleAsync(
                            user,
                            currentManagedRole);
                }


                TempData["AdminUsersError"] =
                    BuildIdentityErrorMessage(
                        addResult,
                        "Không thể cập nhật vai trò mới.");

                return RedirectToIndex(
                    returnSearch,
                    returnRole,
                    returnStatus);
            }


            user.UpdatedAt =
                DateTime.UtcNow;

            await _userManager
                .UpdateAsync(user);

            await _userManager
                .UpdateSecurityStampAsync(
                    user);


            TempData["AdminUsersSuccess"] =
                $"Đã cập nhật vai trò của {user.Email} thành {RoleLabel(newRole)}.";


            return RedirectToIndex(
                returnSearch,
                returnRole,
                returnStatus);
        }


        // =====================================================
        // HELPER
        // =====================================================

        private IActionResult RedirectToIndex(
            string? search,
            string? role,
            string? status)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    search,
                    role,
                    status
                });
        }


        private static string?
            NormalizeRoleFilter(
                string? role)
        {
            return role switch
            {
                "Student" =>
                    "Student",

                "Instructor" =>
                    "Instructor",

                "Admin" =>
                    "Admin",

                _ =>
                    null
            };
        }


        private static string?
            NormalizeStatusFilter(
                string? status)
        {
            return status switch
            {
                "active" =>
                    "active",

                "inactive" =>
                    "inactive",

                _ =>
                    null
            };
        }


        private static string RoleLabel(
            string role)
        {
            return role switch
            {
                "Student" =>
                    "Học viên",

                "Instructor" =>
                    "Giảng viên",

                "Admin" =>
                    "Quản trị viên",

                _ =>
                    role
            };
        }


        private static string
            BuildIdentityErrorMessage(
                IdentityResult result,
                string fallbackMessage)
        {
            var errors =
                result.Errors
                    .Select(e =>
                        e.Description)
                    .Where(e =>
                        !string.IsNullOrWhiteSpace(e))
                    .ToList();


            return errors.Any()
                ? string.Join(
                    " ",
                    errors)
                : fallbackMessage;
        }
    }
}
