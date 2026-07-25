using Femora.Application.Common.Exceptions;
using Femora.Application.Features.Identity.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Femora.API.Middleware;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException ex => new ValidationProblemDetails(
                                        ex.Errors.GroupBy(g => g.PropertyName)
                                        .ToDictionary(g => g.Key,
                                             g => g.Select(e => e.ErrorMessage).ToArray()
                                        )
                                    )
            {
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest
            },

            NotFoundException ex => new ProblemDetails
            {
                Title = "Resource not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
            },

            InvalidOperationException ex => new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
            },

            ContentNotIndexedException ex => new ProblemDetails
            {
                Title = "Content not indexed yet",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
            },

            AlreadyEnrolledException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            },

            PaymentRequiredException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status402PaymentRequired
            },

            CourseNotPublishedException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            },

            EmptyCartException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            },

            ForbiddenException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status403Forbidden
            },

            QuizNotFoundException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status404NotFound
            },

            QuizNotPassedException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            },

            NoNextModuleException ex => new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            },

            AuthenticationException ex => new ProblemDetails
            {
                Title = "Authentication Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            },

            InvalidTokenException ex => new ProblemDetails
            {
                Title = "Invalid Token",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            },

            EmailAlreadyExistsException ex => new ProblemDetails
            {
                Title = "Email Already Exist",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
            },

            ProfileNoLongerAvailableException ex => new ProblemDetails
            {
                Title = "Profile Revoked",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            },

            RegistrationFailedException ex => new ProblemDetails
            {
                Title = "Register Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            },

            InvalidProfileSelectionException ex => new ProblemDetails
            {
                Title = "Invalid Profile Selection",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            },

            // Thrown when a row we tried to update/delete no longer matches what we loaded
            // (e.g. it was already removed or changed by another request in the meantime).
            // Surface this as a clear, actionable 409 instead of a raw 500 with an EF stack trace.
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => new ProblemDetails
            {
                Title = "Item Already Changed",
                Detail = "This item was already updated or removed. Please refresh and try again.",
                Status = StatusCodes.Status409Conflict
            },

            _ => new ProblemDetails
            {
                Title = "Server Error!",
                Detail = $"{exception.GetType().Name}: {exception.Message} | {exception.InnerException?.Message}",
                Status = StatusCodes.Status500InternalServerError
            }
        };

        httpContext.Response.StatusCode = problem.Status!.Value;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });

        return true;
    }
}
