using Femora.Application.Common.DTOs;
using Femora.Application.Features.Ai.Commands.ChatWithLesson;
using Femora.Application.Features.Ai.Commands.DeleteConversation;
using Femora.Application.Features.Ai.Commands.GenerateLessonKeyQuestionsPdf;
using Femora.Application.Features.Ai.Commands.RenameConversation;
using Femora.Application.Features.Ai.Commands.SendMessage;
using Femora.Application.Features.Ai.Commands.SummarizeLesson;
using Femora.Application.Features.Ai.Queries.SuggestedQuestions;
using Femora.Application.Features.Ai.Queries.GetConversation;
using Femora.Application.Features.Ai.Queries.GetConversations;
using Femora.Application.Features.Identity.Commands.SetUserInterests;
using Femora.Application.Features.Identity.Common.Policies;
using Femora.Application.Features.Identity.Queries.GetMyInterests;
using Femora.Application.Features.LMS.Queries.RecommendCourses;
using Femora.Application.Features.Marketplace.Commands.SuggestProductPrice;
using Femora.Application.Features.Marketplace.Queries.RecommendProducts;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.API.Controllers.Ai;

[Route("api/ai")]
[ApiController]
[Authorize(Policy = Policies.NotAdmin)]
public class AiController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim not found."));

    // ============================================================
    // Chatbot (general)
    // ============================================================

    /// <summary>
    /// General chatbot - send a message and get a reply. Creates a new conversation
    /// if conversationId is omitted, otherwise continues the existing one.
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(SendMessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SendMessageCommand
        {
            UserId = CurrentUserId,
            ConversationId = request.ConversationId,
            Message = request.Message
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists all conversations for the current user (for a chat history sidebar).
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetConversationsQuery { UserId = CurrentUserId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single conversation with its full message history.
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}")]
    [ProducesResponseType(typeof(GetConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(
        [FromRoute] Guid conversationId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetConversationQuery { ConversationId = conversationId, UserId = CurrentUserId },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Renames a conversation belonging to the current user.
    /// </summary>
    [HttpPatch("conversations/{conversationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameConversation(
        [FromRoute] Guid conversationId,
        [FromBody] RenameConversationRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RenameConversationCommand { ConversationId = conversationId, UserId = CurrentUserId, Title = request.Title },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a conversation (and all its messages) belonging to the current user.
    /// </summary>
    [HttpDelete("conversations/{conversationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(
        [FromRoute] Guid conversationId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteConversationCommand { ConversationId = conversationId, UserId = CurrentUserId },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// A small rotating set of "quick question" chips for the chatbot's empty state.
    /// Tapping one sends it straight through /api/ai/chat; the set changes every ~3 hours.
    /// </summary>
    [HttpGet("suggested-questions")]
    [ProducesResponseType(typeof(List<Femora.Application.Common.DTOs.SuggestedQuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestedQuestions(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetSuggestedQuestionsQuery { UserId = CurrentUserId, Count = count },
            cancellationToken);
        return Ok(result);
    }

    // ============================================================
    // Lesson RAG (chat-with-lesson + summarize)
    // ============================================================

    /// <summary>
    /// A small rotating set of "quick question" chips for this lesson's Q&amp;A panel,
    /// templated around the lesson's own title. Tapping one sends it straight through
    /// /api/ai/lessons/{lessonId}/chat; the set changes every ~3 hours.
    /// </summary>
    [HttpGet("lessons/{lessonId:guid}/suggested-questions")]
    [ProducesResponseType(typeof(List<Femora.Application.Common.DTOs.SuggestedQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonSuggestedQuestions(
        [FromRoute] Guid lessonId,
        [FromQuery] int count = 4,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetLessonSuggestedQuestionsQuery { LessonId = lessonId, Count = count },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// RAG chat scoped to a specific lesson's indexed content.
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/chat")]
    [ProducesResponseType(typeof(ChatWithLessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChatWithLesson(
        [FromRoute] Guid lessonId,
        [FromBody] ChatWithLessonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChatWithLessonCommand
        {
            UserId = CurrentUserId,
            LessonId = lessonId,
            ConversationId = request.ConversationId,
            Question = request.Question
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Summarizes a lesson's indexed content using RAG retrieval.
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/summarize")]
    [ProducesResponseType(typeof(SummarizeLessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SummarizeLesson(
        [FromRoute] Guid lessonId,
        [FromQuery] string length = "medium",
        CancellationToken cancellationToken = default)
    {
        var command = new SummarizeLessonCommand
        {
            LessonId = lessonId,
            Length = length
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Generates a downloadable PDF study sheet with the lesson's most important
    /// questions and answers, grounded in its indexed content. Uploads the PDF to
    /// blob storage and returns its URL.
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/key-questions-pdf")]
    [ProducesResponseType(typeof(GenerateLessonKeyQuestionsPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateLessonKeyQuestionsPdf(
        [FromRoute] Guid lessonId,
        [FromQuery] int questionCount = 8,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateLessonKeyQuestionsPdfCommand
        {
            LessonId = lessonId,
            QuestionCount = questionCount
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ============================================================
    // Pricing assistant (Seller - Add Product flow)
    // ============================================================

    /// <summary>
    /// Suggests a fair market price (EGP) for a new product based on the Egyptian market.
    /// Intended for use on the "Add Product" form before the seller submits.
    /// </summary>
    [HttpPost("products/suggest-price")]
    [ProducesResponseType(typeof(AISuggestedPrice), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestProductPrice(
        [FromBody] SuggestProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ============================================================
    // Interests (onboarding / profile) + Recommendations
    // ============================================================

    /// <summary>
    /// Returns every course/product category, each flagged with whether the current
    /// user already selected it - used to prefill the "edit my interests" screen.
    /// </summary>
    [HttpGet("interests")]
    [ProducesResponseType(typeof(MyInterestsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyInterests(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyInterestsQuery { UserId = CurrentUserId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sets the current user's preferred onboarding interests.
    /// Call this after registration (onboarding) or from the profile settings page,
    /// and again any time the user wants to update their interests (replaces the
    /// full set every time - send the complete desired list, not just the delta).
    /// </summary>
    [HttpPost("interests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetInterests(
        [FromBody] SetInterestsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetUserInterestsCommand
        {
            UserId = CurrentUserId,
            InterestIds = request.InterestIds
        };

        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Returns AI-ranked product recommendations for the current user,
    /// based on their preferred product categories (set via /api/ai/interests).
    /// </summary>
    [HttpGet("recommendations/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RecommendProducts(
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new RecommendProductsQuery { UserId = CurrentUserId, Top = top },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns AI-ranked course recommendations for the current trainee,
    /// based on their preferred course categories (set via /api/ai/interests).
    /// Returns 404 if the current user does not have a TraineeProfile.
    /// (No "Trainee" Identity role exists in this system yet - this is enforced
    /// inside the handler via TraineeProfile lookup instead of [Authorize(Roles=...)].)
    /// </summary>
    [HttpGet("recommendations/courses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecommendCourses(
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new RecommendCoursesQuery { UserId = CurrentUserId, Top = top },
            cancellationToken);
        return Ok(result);
    }
}
