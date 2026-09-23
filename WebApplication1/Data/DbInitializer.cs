using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            var db =
                serviceProvider.GetRequiredService<ApplicationDbContext>();


            // =====================================
            // 1. TẠO ROLE
            // =====================================

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
                    var result =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Không thể tạo Role {role}.");
                    }
                }
            }


            // =====================================
            // 2. TẠO ADMIN
            // =====================================

            var adminEmail =
                configuration["Admin:Email"];

            var adminPassword =
                configuration["Admin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new Exception(
                    "Chưa cấu hình Admin trong User Secrets.");
            }

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

                var createAdmin =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!createAdmin.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        createAdmin.Errors
                            .Select(e => e.Description));

                    throw new Exception(
                        $"Không thể tạo Admin: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(
                adminUser,
                "Admin"))
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    "Admin");
            }


            // =====================================
            // 3. TẠO / CẬP NHẬT GIẢNG VIÊN MẪU
            // =====================================

            var instructorEmail =
                configuration["InstructorAccount:Email"];

            var instructorPassword =
                configuration["InstructorAccount:Password"];


            if (string.IsNullOrWhiteSpace(instructorEmail) ||
                string.IsNullOrWhiteSpace(instructorPassword))
            {
                throw new Exception(
                    "Chưa cấu hình InstructorAccount trong User Secrets.");
            }


            var instructor =
                await userManager.FindByEmailAsync(
                    instructorEmail);


            if (instructor == null)
            {
                instructor = new ApplicationUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    FullName = "Giảng viên EduLearn",
                    EmailConfirmed = true,
                    IsActive = true,
                    Bio = "Giảng viên phụ trách các khóa học Công nghệ thông tin trên EduLearn.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };


                var createInstructor =
                    await userManager.CreateAsync(
                        instructor,
                        instructorPassword);


                if (!createInstructor.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        createInstructor.Errors
                            .Select(e => e.Description));

                    throw new Exception(
                        $"Không thể tạo giảng viên mẫu: {errors}");
                }
            }
            else
            {
                /*
                 * Đồng bộ mật khẩu của tài khoản mẫu
                 * với mật khẩu trong User Secrets.
                 */

                bool hasPassword =
                    await userManager.HasPasswordAsync(
                        instructor);


                if (!hasPassword)
                {
                    var addPasswordResult =
                        await userManager.AddPasswordAsync(
                            instructor,
                            instructorPassword);


                    if (!addPasswordResult.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            addPasswordResult.Errors
                                .Select(e => e.Description));

                        throw new Exception(
                            $"Không thể thêm mật khẩu cho giảng viên: {errors}");
                    }
                }
                else
                {
                    bool passwordMatches =
                        await userManager.CheckPasswordAsync(
                            instructor,
                            instructorPassword);


                    if (!passwordMatches)
                    {
                        var removePasswordResult =
                            await userManager.RemovePasswordAsync(
                                instructor);


                        if (!removePasswordResult.Succeeded)
                        {
                            var errors = string.Join(
                                ", ",
                                removePasswordResult.Errors
                                    .Select(e => e.Description));

                            throw new Exception(
                                $"Không thể đặt lại mật khẩu giảng viên: {errors}");
                        }


                        var addPasswordResult =
                            await userManager.AddPasswordAsync(
                                instructor,
                                instructorPassword);


                        if (!addPasswordResult.Succeeded)
                        {
                            var errors = string.Join(
                                ", ",
                                addPasswordResult.Errors
                                    .Select(e => e.Description));

                            throw new Exception(
                                $"Không thể cập nhật mật khẩu giảng viên: {errors}");
                        }
                    }
                }
            }


            // Đảm bảo tài khoản có role Instructor.
            if (!await userManager.IsInRoleAsync(
                instructor,
                "Instructor"))
            {
                var addRoleResult =
                    await userManager.AddToRoleAsync(
                        instructor,
                        "Instructor");


                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        addRoleResult.Errors
                            .Select(e => e.Description));

                    throw new Exception(
                        $"Không thể gán role Instructor: {errors}");
                }
            }


            // =====================================
            // 4. TẠO DANH MỤC KHÓA HỌC
            // =====================================

            var categories = new[]
            {
                new Category
                {
                    Name = "ASP.NET Core",
                    Slug = "aspnet-core",
                    Description = "Các khóa học ASP.NET Core và C#.",
                    Icon = ".NET",
                    DisplayOrder = 1
                },

                new Category
                {
                    Name = "Lập trình Web",
                    Slug = "lap-trinh-web",
                    Description = "Phát triển website và ứng dụng web.",
                    Icon = "WEB",
                    DisplayOrder = 2
                },

                new Category
                {
                    Name = "Cơ sở dữ liệu",
                    Slug = "co-so-du-lieu",
                    Description = "SQL Server và thiết kế cơ sở dữ liệu.",
                    Icon = "DB",
                    DisplayOrder = 3
                },

                new Category
                {
                    Name = "Python",
                    Slug = "python",
                    Description = "Lập trình Python từ cơ bản.",
                    Icon = "PY",
                    DisplayOrder = 4
                },

                new Category
                {
                    Name = "Frontend",
                    Slug = "frontend",
                    Description = "HTML, CSS và JavaScript.",
                    Icon = "JS",
                    DisplayOrder = 5
                },

                new Category
                {
                    Name = "UI/UX",
                    Slug = "ui-ux",
                    Description = "Thiết kế giao diện và trải nghiệm người dùng.",
                    Icon = "UI",
                    DisplayOrder = 6
                },

                new Category
                {
                    Name = "Git & DevOps",
                    Slug = "git-devops",
                    Description = "Git, GitHub và quy trình phát triển phần mềm.",
                    Icon = "Git",
                    DisplayOrder = 7
                },

                new Category
                {
                    Name = "Ngôn ngữ lập trình",
                    Slug = "ngon-ngu-lap-trinh",
                    Description = "Các ngôn ngữ lập trình phổ biến.",
                    Icon = "C#",
                    DisplayOrder = 8
                }
            };


            foreach (var category in categories)
            {
                var exists =
                    await db.Categories.AnyAsync(
                        c => c.Slug == category.Slug);

                if (!exists)
                {
                    db.Categories.Add(category);
                }
            }


            await db.SaveChangesAsync();


            // =====================================
            // 5. TẠO KHÓA HỌC MẪU
            // =====================================

            if (!await db.Courses.AnyAsync())
            {
                var aspNetCategory =
                    await db.Categories.SingleAsync(
                        c => c.Slug == "aspnet-core");

                var programmingCategory =
                    await db.Categories.SingleAsync(
                        c => c.Slug == "ngon-ngu-lap-trinh");

                var databaseCategory =
                    await db.Categories.SingleAsync(
                        c => c.Slug == "co-so-du-lieu");

                var gitCategory =
                    await db.Categories.SingleAsync(
                        c => c.Slug == "git-devops");


                var courses = new[]
                {
                    new Course
                    {
                        Title = "Lập trình ASP.NET Core từ cơ bản đến nâng cao",
                        Slug = "lap-trinh-aspnet-core-tu-co-ban-den-nang-cao",
                        ShortDescription =
                            "Xây dựng ứng dụng web hiện đại với ASP.NET Core MVC và Entity Framework Core.",
                        Description =
                            "Khóa học cung cấp kiến thức từ nền tảng ASP.NET Core đến xây dựng ứng dụng web hoàn chỉnh.",
                        Price = 1200000,
                        Level = "Beginner",
                        Language = "vi",
                        Duration = 1200,
                        InstructorId = instructor.Id,
                        CategoryId = aspNetCategory.Id,
                        IsPublished = true,
                        IsFeatured = true,
                        PublishedAt = DateTime.UtcNow
                    },

                    new Course
                    {
                        Title = "C# nền tảng cho người mới bắt đầu",
                        Slug = "csharp-nen-tang-cho-nguoi-moi-bat-dau",
                        ShortDescription =
                            "Làm quen với cú pháp, lập trình hướng đối tượng và tư duy lập trình bằng C#.",
                        Description =
                            "Khóa học dành cho người mới bắt đầu tìm hiểu ngôn ngữ lập trình C#.",
                        Price = 790000,
                        Level = "Beginner",
                        Language = "vi",
                        Duration = 900,
                        InstructorId = instructor.Id,
                        CategoryId = programmingCategory.Id,
                        IsPublished = true,
                        IsFeatured = true,
                        PublishedAt = DateTime.UtcNow
                    },

                    new Course
                    {
                        Title = "SQL Server và thiết kế cơ sở dữ liệu",
                        Slug = "sql-server-va-thiet-ke-co-so-du-lieu",
                        ShortDescription =
                            "Học SQL Server, truy vấn dữ liệu và thiết kế cơ sở dữ liệu quan hệ.",
                        Description =
                            "Khóa học cung cấp kiến thức về SQL Server từ truy vấn cơ bản đến thiết kế cơ sở dữ liệu.",
                        Price = 690000,
                        Level = "Intermediate",
                        Language = "vi",
                        Duration = 840,
                        InstructorId = instructor.Id,
                        CategoryId = databaseCategory.Id,
                        IsPublished = true,
                        IsFeatured = true,
                        PublishedAt = DateTime.UtcNow
                    },

                    new Course
                    {
                        Title = "Git & GitHub cho lập trình viên",
                        Slug = "git-github-cho-lap-trinh-vien",
                        ShortDescription =
                            "Quản lý source code và làm việc nhóm hiệu quả với Git và GitHub.",
                        Description =
                            "Khóa học hướng dẫn sử dụng Git, GitHub, branch, commit và quy trình quản lý source code.",
                        Price = 490000,
                        Level = "Beginner",
                        Language = "vi",
                        Duration = 480,
                        InstructorId = instructor.Id,
                        CategoryId = gitCategory.Id,
                        IsPublished = true,
                        IsFeatured = true,
                        PublishedAt = DateTime.UtcNow
                    }
                };


                db.Courses.AddRange(courses);

                await db.SaveChangesAsync();
            }
        }
    }
}