using Femora.Application.Features.LMS.Quizzes.Commands;
using GenerateQuizAppResponse = Femora.Application.Features.LMS.Quizzes.Commands.GenerateQuiz.GenerateQuizResponse;
using Femora.Application.Features.LMS.Quizzes.Commands.GenerateQuiz;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using GetQuizAppResponse = Femora.Application.Features.LMS.Quizzes.Queries.GetQuiz.GetQuizResponse;
using Femora.Application.Features.LMS.Quizzes.Queries.GetQuiz;
using Femora.API.Controllers.LMS.Requests;
using Femora.API.Controllers.LMS.Responses;
using Femora.Application.Features.LMS.Quizzes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GenerateQuizResponse = Femora.API.Controllers.LMS.Responses.GenerateQuizResponse;
using QuestionResponse = Femora.API.Controllers.LMS.Responses.QuestionResponse;
using ChoiceResponse = Femora.API.Controllers.LMS.Responses.ChoiceResponse;

namespace Femora.API.Controllers.LMS;

[Route("api/quizzes")]
[ApiController]
[Authorize]
public class QuizController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Generate a new AI quiz for a module using the RAG pipeline.
    /// Pulls indexed lesson chunks from Azure AI Search, sends them to
    /// Azure OpenAI, persists the resulting Quiz + Questions + Choices,
    /// and returns the full quiz with correct answers (instructor view).
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateQuizAppResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateQuizCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(new GenerateQuizResponse
        {
            QuizId = result.QuizId,
            Title = result.Title,
            Description = string.Empty,
            Questions = result.Questions.Select(question => new QuestionResponse
            {
                QuestionId = question.QuestionId,
                Text = question.Text,
                Type = question.Type,
                Choices = question.Choices.Select(choice => new ChoiceResponse
                {
                    ChoiceId = choice.ChoiceId,
                    Text = choice.Text,
                    Order = 0,
                    IsCorrect = choice.IsCorrect
                }).ToList()
            }).ToList()
        });
    }

    /// <summary>
    /// Get a quiz by ID.
    /// Choices are returned WITHOUT the IsCorrect flag â€” this is the
    /// trainee-safe view intended for the quiz-taking UI.
    /// </summary>
    [HttpGet("{quizId:guid}")]
    [ProducesResponseType(typeof(GetQuizAppResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid quizId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetQuizQuery { QuizId = quizId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submit a quiz attempt.
    /// Scores the answers, persists the QuizAttempt + QuizAttemptAnswers,
    /// and returns the score, pass/fail status, and per-question results.
    /// </summary>
    [HttpPost("{quizId:guid}/submit")]
    [ProducesResponseType(typeof(SubmitQuizResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(
        [FromRoute] Guid quizId,
        [FromBody] SubmitQuizRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitQuizCommand(
            quizId,
            request.EnrollmentId,
            request.Answers);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// AI-generated "weak points" review of the trainee's last attempt, available only
    /// after they've exhausted every regular attempt on this quiz. Calling this the first
    /// time also unlocks exactly one bonus attempt (subsequent calls just re-fetch the
    /// same review without granting anything else).
    /// </summary>
    [HttpGet("{quizId:guid}/weak-points-report")]
    [ProducesResponseType(typeof(Femora.Application.Features.LMS.Quizzes.DTOs.QuizWeakPointsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeakPointsReport(
        [FromRoute] Guid quizId,
        [FromQuery] Guid enrollmentId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim not found."));

        var result = await mediator.Send(
            new GetQuizWeakPointsQuery { QuizId = quizId, EnrollmentId = enrollmentId, UserId = userId },
            cancellationToken);
        return Ok(result);
    }
}




