namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class ChoiceDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}