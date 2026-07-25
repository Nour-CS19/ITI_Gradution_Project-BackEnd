namespace Femora.API.Controllers.LMS.Responses;

public class SubmitQuizResponse
{
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public List<AnswerResultResponse> AnswerResults { get; set; } = new();
}

public class AnswerResultResponse
{
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
}
