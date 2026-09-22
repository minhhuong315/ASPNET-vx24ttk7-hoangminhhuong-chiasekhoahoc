using Microsoft.AspNetCore.Identity;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tạo các Role mặc định
            string[] roles =
            {
                "Student",
                "Instructor",
                "Admin"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            $"Không thể tạo Role {role}.");
                    }
                }
            }

            // 2. Đọc tài khoản Admin từ User Secrets
            var adminEmail = configuration["Admin:Email"];
            var adminPassword = configuration["Admin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new Exception(
                    "Chưa cấu hình Admin:Email hoặc Admin:Password trong User Secrets.");
            }

            // 3. Kiểm tra Admin đã tồn tại chưa
            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên EduLearn",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        createResult.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Không thể tạo tài khoản Admin: {errors}");
                }
            }

            // 4. Gán Role Admin
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    "Admin");
            }
        }
    }
}