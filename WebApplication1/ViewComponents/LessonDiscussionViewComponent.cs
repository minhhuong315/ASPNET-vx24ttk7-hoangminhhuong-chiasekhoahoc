using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.ViewModels;

namespace OnlineLearningPlatform.ViewComponents
{
    public class LessonDiscussionViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LessonDiscussionViewComponent(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int lessonId)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Module)
                .FirstOrDefaultAsync(l =>
                    l.Id == lessonId);

            if (lesson == null)
            {
                return Content(string.Empty);
            }

            var questions = await _context.LessonQuestions
                .AsNoTracking()
                .Where(q =>
                    q.LessonId == lessonId)
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .OrderByDescending(q =>
                    q.CreatedAt)
                .ToListAsync();

            var model =
                new LessonDiscussionViewModel
                {
                    LessonId = lessonId,
                    CourseId = lesson.Module.CourseId,
                    Questions = questions
                };

            return View(model);
        }
    }
}