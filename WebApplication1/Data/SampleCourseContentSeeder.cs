using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Data
{
    public static class SampleCourseContentSeeder
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            var context =
                serviceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedAspNetCourseAsync(context);
            await SeedCSharpCourseAsync(context);
            await SeedSqlCourseAsync(context);
            await SeedGitCourseAsync(context);
        }


        // =========================================
        // ASP.NET CORE
        // =========================================

        private static async Task SeedAspNetCourseAsync(
            ApplicationDbContext context)
        {
            await SeedCourseAsync(
                context,
                "lap-trinh-aspnet-core-tu-co-ban-den-nang-cao",

                new ModuleSeed(
                    1,
                    "Chương 1: Làm quen với ASP.NET Core",
                    "Kiến thức nền tảng và cấu trúc dự án ASP.NET Core.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Giới thiệu ASP.NET Core",
                            "Tổng quan về ASP.NET Core, .NET và các thành phần chính của một ứng dụng web.",
                            15,
                            true),

                        new LessonSeed(
                            2,
                            "Cấu trúc dự án ASP.NET Core",
                            "Tìm hiểu Program.cs, Controllers, Models, Views và thư mục wwwroot.",
                            20,
                            false)
                    }),

                new ModuleSeed(
                    2,
                    "Chương 2: ASP.NET Core MVC",
                    "Tìm hiểu mô hình MVC và cách xây dựng ứng dụng web.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Mô hình MVC",
                            "Tìm hiểu vai trò của Model, View và Controller trong ASP.NET Core MVC.",
                            25,
                            false),

                        new LessonSeed(
                            2,
                            "Routing và Controller",
                            "Tìm hiểu cách định tuyến request đến Controller và Action phù hợp.",
                            25,
                            false)
                    }),

                new ModuleSeed(
                    3,
                    "Chương 3: Entity Framework Core",
                    "Làm việc với cơ sở dữ liệu bằng Entity Framework Core.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "DbContext và Model",
                            "Tìm hiểu DbContext, DbSet và cách ánh xạ Model vào cơ sở dữ liệu.",
                            30,
                            false),

                        new LessonSeed(
                            2,
                            "Migration và truy vấn dữ liệu",
                            "Thực hành Migration và các truy vấn dữ liệu cơ bản với Entity Framework Core.",
                            30,
                            false)
                    }),

                new ModuleSeed(
                    4,
                    "Chương 4: Authentication và hoàn thiện ứng dụng",
                    "Xây dựng chức năng tài khoản và hoàn thiện ứng dụng ASP.NET Core.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "ASP.NET Core Identity",
                            "Tìm hiểu đăng ký, đăng nhập, đăng xuất và phân quyền người dùng.",
                            35,
                            false),

                        new LessonSeed(
                            2,
                            "Hoàn thiện và triển khai ứng dụng",
                            "Kiểm tra chức năng, xử lý lỗi và chuẩn bị ứng dụng để triển khai.",
                            30,
                            false)
                    }));
        }


        // =========================================
        // C#
        // =========================================

        private static async Task SeedCSharpCourseAsync(
            ApplicationDbContext context)
        {
            await SeedCourseAsync(
                context,
                "csharp-nen-tang-cho-nguoi-moi-bat-dau",

                new ModuleSeed(
                    1,
                    "Chương 1: C# căn bản",
                    "Làm quen với cú pháp và dữ liệu trong C#.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Biến và kiểu dữ liệu",
                            "Tìm hiểu biến, hằng số và các kiểu dữ liệu phổ biến trong C#.",
                            18,
                            true),

                        new LessonSeed(
                            2,
                            "Câu lệnh điều kiện và vòng lặp",
                            "Thực hành if, switch, for, while và foreach.",
                            25,
                            false)
                    }),

                new ModuleSeed(
                    2,
                    "Chương 2: Lập trình hướng đối tượng",
                    "Các kiến thức nền tảng về OOP trong C#.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Class và Object",
                            "Tìm hiểu cách khai báo lớp, tạo đối tượng, thuộc tính và phương thức.",
                            25,
                            false),

                        new LessonSeed(
                            2,
                            "Kế thừa và đa hình",
                            "Giới thiệu các nguyên lý kế thừa và đa hình trong lập trình hướng đối tượng.",
                            30,
                            false)
                    }),

                new ModuleSeed(
                    3,
                    "Chương 3: Collection và xử lý dữ liệu",
                    "Làm việc với các cấu trúc dữ liệu thông dụng trong C#.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Array và List",
                            "Tìm hiểu mảng, List và cách quản lý tập hợp dữ liệu.",
                            25,
                            false),

                        new LessonSeed(
                            2,
                            "LINQ cơ bản",
                            "Sử dụng LINQ để lọc, sắp xếp và truy vấn dữ liệu trong C#.",
                            30,
                            false)
                    }),

                new ModuleSeed(
                    4,
                    "Chương 4: Xử lý lỗi và ứng dụng thực tế",
                    "Hoàn thiện kỹ năng C# thông qua xử lý lỗi và bài tập thực hành.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Exception Handling",
                            "Tìm hiểu try, catch, finally và cách xử lý ngoại lệ.",
                            30,
                            false),

                        new LessonSeed(
                            2,
                            "Xây dựng chương trình C# hoàn chỉnh",
                            "Vận dụng các kiến thức đã học để xây dựng một chương trình C# nhỏ.",
                            35,
                            false)
                    }));
        }


        // =========================================
        // SQL SERVER
        // =========================================

        private static async Task SeedSqlCourseAsync(
            ApplicationDbContext context)
        {
            await SeedCourseAsync(
                context,
                "sql-server-va-thiet-ke-co-so-du-lieu",

                new ModuleSeed(
                    1,
                    "Chương 1: Cơ sở dữ liệu quan hệ",
                    "Làm quen với SQL Server và mô hình dữ liệu quan hệ.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Giới thiệu SQL Server",
                            "Tổng quan SQL Server và vai trò của hệ quản trị cơ sở dữ liệu.",
                            15,
                            true),

                        new LessonSeed(
                            2,
                            "Thiết kế bảng và khóa",
                            "Tìm hiểu bảng, khóa chính, khóa ngoại và mối quan hệ giữa các bảng.",
                            25,
                            false)
                    }),

                new ModuleSeed(
                    2,
                    "Chương 2: Truy vấn dữ liệu",
                    "Thực hành các câu lệnh SQL phổ biến.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "SELECT và lọc dữ liệu",
                            "Sử dụng SELECT, WHERE và ORDER BY để truy vấn dữ liệu.",
                            25,
                            false),

                        new LessonSeed(
                            2,
                            "JOIN nhiều bảng",
                            "Tìm hiểu INNER JOIN, LEFT JOIN và cách kết hợp dữ liệu từ nhiều bảng.",
                            30,
                            false)
                    }),

                new ModuleSeed(
                    3,
                    "Chương 3: Thiết kế cơ sở dữ liệu",
                    "Thiết kế dữ liệu hợp lý và hạn chế dư thừa.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Mối quan hệ giữa các bảng",
                            "Tìm hiểu quan hệ một-một, một-nhiều và nhiều-nhiều.",
                            30,
                            false),

                        new LessonSeed(
                            2,
                            "Chuẩn hóa dữ liệu",
                            "Giới thiệu các nguyên tắc chuẩn hóa cơ sở dữ liệu.",
                            35,
                            false)
                    }),

                new ModuleSeed(
                    4,
                    "Chương 4: SQL nâng cao",
                    "Một số kỹ thuật SQL thường dùng trong ứng dụng thực tế.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "GROUP BY và hàm tổng hợp",
                            "Sử dụng COUNT, SUM, AVG, GROUP BY và HAVING.",
                            30,
                            false),

                        new LessonSeed(
                            2,
                            "View và Stored Procedure",
                            "Giới thiệu View và Stored Procedure trong SQL Server.",
                            35,
                            false)
                    }));
        }


        // =========================================
        // GIT & GITHUB
        // =========================================

        private static async Task SeedGitCourseAsync(
            ApplicationDbContext context)
        {
            await SeedCourseAsync(
                context,
                "git-github-cho-lap-trinh-vien",

                new ModuleSeed(
                    1,
                    "Chương 1: Làm quen với Git",
                    "Các kiến thức và thao tác Git cơ bản.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Git là gì?",
                            "Tìm hiểu Git, version control và vai trò của Git trong quá trình phát triển phần mềm.",
                            12,
                            true),

                        new LessonSeed(
                            2,
                            "Commit và lịch sử thay đổi",
                            "Thực hành git add, git commit và xem lịch sử thay đổi của dự án.",
                            20,
                            false)
                    }),

                new ModuleSeed(
                    2,
                    "Chương 2: Làm việc với GitHub",
                    "Quản lý source code và repository trên GitHub.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Repository trên GitHub",
                            "Tạo repository, kết nối remote và đẩy source code lên GitHub.",
                            20,
                            false),

                        new LessonSeed(
                            2,
                            "Branch và làm việc nhóm",
                            "Tìm hiểu branch và quy trình làm việc nhóm cơ bản.",
                            25,
                            false)
                    }),

                new ModuleSeed(
                    3,
                    "Chương 3: Branch và Merge",
                    "Quản lý các nhánh phát triển và hợp nhất source code.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Tạo và quản lý Branch",
                            "Thực hành tạo, chuyển đổi và quản lý các branch trong Git.",
                            25,
                            false),

                        new LessonSeed(
                            2,
                            "Merge và xử lý Conflict",
                            "Tìm hiểu merge và cách xử lý xung đột source code.",
                            30,
                            false)
                    }),

                new ModuleSeed(
                    4,
                    "Chương 4: Cộng tác trên GitHub",
                    "Sử dụng GitHub hiệu quả trong quá trình làm việc nhóm.",
                    new[]
                    {
                        new LessonSeed(
                            1,
                            "Pull Request và Code Review",
                            "Tìm hiểu Pull Request, review code và quy trình hợp nhất thay đổi.",
                            25,
                            false),

                        new LessonSeed(
                            2,
                            "Quy trình làm việc nhóm với GitHub",
                            "Thực hành quy trình branch, commit, push, pull request và merge.",
                            30,
                            false)
                    }));
        }


        // =========================================
        // HÀM SEED CHUNG
        // =========================================

        private static async Task SeedCourseAsync(
            ApplicationDbContext context,
            string courseSlug,
            params ModuleSeed[] moduleSeeds)
        {
            var course = await context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c =>
                    c.Slug == courseSlug);

            if (course == null)
            {
                return;
            }

            foreach (var moduleSeed in moduleSeeds)
            {
                var module = course.Modules
                    .FirstOrDefault(m =>
                        m.DisplayOrder ==
                        moduleSeed.DisplayOrder);

                if (module == null)
                {
                    module = new Module
                    {
                        CourseId = course.Id,
                        Title = moduleSeed.Title,
                        Description =
                            moduleSeed.Description,
                        DisplayOrder =
                            moduleSeed.DisplayOrder,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Modules.Add(module);

                    await context.SaveChangesAsync();
                }
                else
                {
                    module.Title =
                        moduleSeed.Title;

                    module.Description =
                        moduleSeed.Description;
                }

                foreach (var lessonSeed
                    in moduleSeed.Lessons)
                {
                    var lesson = module.Lessons
                        .FirstOrDefault(l =>
                            l.Title ==
                            lessonSeed.Title);

                    if (lesson == null)
                    {
                        lesson = new Lesson
                        {
                            ModuleId = module.Id,
                            Title = lessonSeed.Title,
                            Content = lessonSeed.Content,
                            Duration =
                                lessonSeed.Duration,
                            DisplayOrder =
                                lessonSeed.DisplayOrder,
                            IsFree = lessonSeed.IsFree,
                            CreatedAt =
                                DateTime.UtcNow
                        };

                        context.Lessons.Add(lesson);
                    }
                    else
                    {
                        lesson.Content =
                            lessonSeed.Content;

                        lesson.Duration =
                            lessonSeed.Duration;

                        lesson.DisplayOrder =
                            lessonSeed.DisplayOrder;

                        lesson.IsFree =
                            lessonSeed.IsFree;
                    }
                }

                await context.SaveChangesAsync();
            }

            // Thời lượng khóa học được tính
            // theo tổng thời lượng các bài thực tế.
            course.Duration =
                await context.Lessons
                    .Where(l =>
                        l.Module.CourseId ==
                        course.Id)
                    .SumAsync(l => l.Duration);

            await context.SaveChangesAsync();
        }


        // =========================================
        // DỮ LIỆU TRUNG GIAN
        // =========================================

        private sealed class ModuleSeed
        {
            public int DisplayOrder { get; }

            public string Title { get; }

            public string Description { get; }

            public LessonSeed[] Lessons { get; }

            public ModuleSeed(
                int displayOrder,
                string title,
                string description,
                LessonSeed[] lessons)
            {
                DisplayOrder = displayOrder;
                Title = title;
                Description = description;
                Lessons = lessons;
            }
        }


        private sealed class LessonSeed
        {
            public int DisplayOrder { get; }

            public string Title { get; }

            public string Content { get; }

            public int Duration { get; }

            public bool IsFree { get; }

            public LessonSeed(
                int displayOrder,
                string title,
                string content,
                int duration,
                bool isFree)
            {
                DisplayOrder = displayOrder;
                Title = title;
                Content = content;
                Duration = duration;
                IsFree = isFree;
            }
        }
    }
}