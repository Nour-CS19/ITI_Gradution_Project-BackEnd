using MediatR;
using Femora.Application.Features.LMS.Quizzes.DTOs;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record ReorderQuestionsCommand(
    Guid QuizId,
    List<QuestionOrderDto> Questions
) : IRequest;

