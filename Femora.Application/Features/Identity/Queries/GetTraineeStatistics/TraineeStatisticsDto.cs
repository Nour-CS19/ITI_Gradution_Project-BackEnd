namespace Femora.Application.Features.Identity.Queries.GetTraineeStatistics
{
    public class TraineeStatisticsDto
    {
        /// <summary>
        /// Total number of quiz attempts made by the trainee.
        /// </summary>
        public int QuizzesAttempted { get; set; }

        /// <summary>
        /// Number of quizzes passed (score >= passing threshold).
        /// </summary>
        public int QuizzesPassed { get; set; }

        /// <summary>
        /// Number of quizzes failed (score < passing threshold).
        /// </summary>
        public int QuizzesFailed { get; set; }

        /// <summary>
        /// Total number of courses the trainee is enrolled in.
        /// </summary>
        public int EnrolledCoursesCount { get; set; }

        /// <summary>
        /// Number of courses the trainee has completed.
        /// </summary>
        public int CompletedCoursesCount { get; set; }

        /// <summary>
        /// Total number of lessons the trainee has completed.
        /// </summary>
        public int CompletedLessonsCount { get; set; }

        /// <summary>
        /// Number of learning goals created by the trainee.
        /// </summary>
        public int LearningGoalsCount { get; set; }

        /// <summary>
        /// List of course category IDs the trainee is interested in.
        /// </summary>
        public List<Guid> PreferredCategoryIds { get; set; } = new();

        /// <summary>
        /// List of product category IDs the trainee is interested in.
        /// </summary>
        public List<Guid> PreferredProductCategoryIds { get; set; } = new();
    }
}
