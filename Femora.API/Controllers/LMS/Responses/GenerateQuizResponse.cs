namespace Femora.API.Controllers.LMS.Responses;

public class GenerateQuizResponse
{
    public Guid QuizId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<QuestionResponse> Questions { get; set; } = new();
}

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; }
    public string Type { get; set; } = "MultipleChoice";
    public List<ChoiceResponse> Choices { get; set; } = new();
}

public class ChoiceResponse
{
    public Guid ChoiceId { get; set; }
    public string Text { get; set; }
    public int Order { get; set; }
    // Included only in instructor view
    public bool? IsCorrect { get; set; }
}
