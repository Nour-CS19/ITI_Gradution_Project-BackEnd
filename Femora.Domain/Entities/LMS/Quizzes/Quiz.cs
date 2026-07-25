using Femora.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Femora.Domain.Entities.LMS.Quizzes;

public class Quiz : BaseEntity
{
    [Required]
    public Guid CourseId { get; set; }

    public Guid? ModuleId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int MinimumPassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    [ForeignKey(nameof(ModuleId))]
    public Module? Module { get; set; }

}
