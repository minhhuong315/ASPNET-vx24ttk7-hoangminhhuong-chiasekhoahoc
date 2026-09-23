using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        // =========================
        // DB SETS
        // =========================

        public DbSet<Category> Categories
            => Set<Category>();

        public DbSet<Course> Courses
            => Set<Course>();

        public DbSet<Module> Modules
            => Set<Module>();

        public DbSet<Lesson> Lessons
            => Set<Lesson>();

        public DbSet<Enrollment> Enrollments
            => Set<Enrollment>();

        public DbSet<LessonProgress> LessonProgresses
            => Set<LessonProgress>();

        public DbSet<Review> Reviews
            => Set<Review>();

        public DbSet<Wishlist> Wishlists
            => Set<Wishlist>();


        // HỎI ĐÁP BÀI HỌC

        public DbSet<LessonQuestion> LessonQuestions
            => Set<LessonQuestion>();

        public DbSet<LessonAnswer> LessonAnswers
            => Set<LessonAnswer>();


        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // =========================
            // CATEGORY
            // =========================

            builder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();


            builder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // COURSE
            // =========================

            builder.Entity<Course>()
                .HasIndex(c => c.Slug)
                .IsUnique();


            builder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // MODULE
            // =========================

            builder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // LESSON
            // =========================

            builder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // ENROLLMENT
            // =========================

            builder.Entity<Enrollment>()
                .HasKey(e => new
                {
                    e.UserId,
                    e.CourseId
                });


            builder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // LESSON PROGRESS
            // =========================

            builder.Entity<LessonProgress>()
                .HasKey(p => new
                {
                    p.UserId,
                    p.LessonId
                });


            builder.Entity<LessonProgress>()
                .HasOne(p => p.User)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<LessonProgress>()
                .HasOne(p => p.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // REVIEW
            // =========================

            builder.Entity<Review>()
                .HasIndex(r => new
                {
                    r.UserId,
                    r.CourseId
                })
                .IsUnique();


            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // WISHLIST
            // =========================

            builder.Entity<Wishlist>()
                .HasKey(w => new
                {
                    w.UserId,
                    w.CourseId
                });


            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Wishlist>()
                .HasOne(w => w.Course)
                .WithMany(c => c.Wishlists)
                .HasForeignKey(w => w.CourseId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // LESSON QUESTION
            // =========================

            builder.Entity<LessonQuestion>()
                .HasOne(q => q.Lesson)
                .WithMany()
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<LessonQuestion>()
                .HasOne(q => q.User)
                .WithMany()
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // LESSON ANSWER
            // =========================

            builder.Entity<LessonAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<LessonAnswer>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}