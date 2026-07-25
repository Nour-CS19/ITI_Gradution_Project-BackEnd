namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public List<ChoiceDto> Choices { get; set; } = new();
}