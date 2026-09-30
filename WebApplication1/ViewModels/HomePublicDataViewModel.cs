namespace OnlineLearningPlatform.ViewModels
{
    public class HomePublicDataViewModel
    {
        public int PublishedCourseCount { get; set; }

        public int StudentCount { get; set; }

        public int InstructorCount { get; set; }

        public double AverageRating { get; set; }


        public List<HomeCategoryItemViewModel>
            Categories
        { get; set; }
                = new List<HomeCategoryItemViewModel>();


        public List<HomeFeaturedCourseViewModel>
            FeaturedCourses
        { get; set; }
                = new List<HomeFeaturedCourseViewModel>();
    }


    public class HomeCategoryItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string Icon { get; set; }
            = string.Empty;

        public int PublishedCourseCount { get; set; }
    }


    public class HomeFeaturedCourseViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Slug { get; set; }
            = string.Empty;

        public string CategoryName { get; set; }
            = string.Empty;

        public string CategoryIcon { get; set; }
            = string.Empty;

        public string InstructorName { get; set; }
            = string.Empty;

        public string Level { get; set; }
            = string.Empty;

        public int EnrollmentCount { get; set; }

        public bool IsFeatured { get; set; }
    }
}
