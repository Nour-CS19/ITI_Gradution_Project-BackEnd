using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using Femora.Domain.Entities.LMS.Quizzes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class SubmitQuizHandler : IRequestHandler<SubmitQuizCommand, SubmitQuizResultDto>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitQuizHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubmitQuizResultDto> Handle(SubmitQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken);

        if (quiz == null)
            throw new NotFoundException(nameof(Quiz), request.QuizId.ToString());

        // QuizAttempt.TraineeProfileId is a required FK - it must point to a real
        // TraineeProfile row, otherwise SaveChangesAsync throws a DbUpdateException
        // from the FK constraint (this was previously left at Guid.Empty).
        var traineeProfile = await _context.TraineeProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("TraineeProfile", _currentUser.UserId.ToString());

        var previousAttemptsCount = await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quiz.Id && a.EnrollmentId == request.EnrollmentId, cancellationToken);

        // A trainee who exhausted the regular MaxAttempts can unlock exactly one bonus
        // attempt by reading the AI weak-points review (see GetQuizWeakPointsHandler).
        // That shows up here as an unused QuizRetryGrant row.
        var unusedGrant = await _context.QuizRetryGrants
            .Where(g => g.QuizId == quiz.Id && g.EnrollmentId == request.EnrollmentId && !g.IsUsed)
            .OrderBy(g => g.GrantedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var allowedAttempts = quiz.MaxAttempts + (unusedGrant != null ? 1 : 0);

        if (previousAttemptsCount >= allowedAttempts)
            throw new InvalidOperationException("Maximum number of attempts reached for this quiz.");

        var usingGrant = previousAttemptsCount >= quiz.MaxAttempts && unusedGrant != null;

        var questionsById = quiz.Questions.ToDictionary(q => q.Id);
        var maxScore = quiz.Questions.Count;
        var correctCount = 0;
        var answerRecords = new List<QuizAttemptAnswer>();

        foreach (var answer in request.Answers.GroupBy(a => a.QuestionId).Select(g => g.First()))
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question))
                continue;

            var choice = question.Choices.FirstOrDefault(c => c.Id == answer.ChoiceId);
            if (choice == null)
                continue;

            if (choice.IsCorrect)
                correctCount++;

            answerRecords.Add(new QuizAttemptAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                ChoiceId = choice.Id,
                IsCorrect = choice.IsCorrect
            });
        }

        // NOTE: Score/MaxScore are "correct answers out of total questions" (e.g. 7/10),
        // NOT a percentage - keeping units consistent is what the frontend displays as
        // "score / maxScore". Percentage is the separate 0-100 value used for pass/fail.
        decimal percentage = maxScore > 0
            ? Math.Round((decimal)correctCount / maxScore * 100, 2)
            : 0;
        bool passed = percentage >= quiz.MinimumPassingScore;

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            EnrollmentId = request.EnrollmentId,
            TraineeProfileId = traineeProfile.Id,
            Score = correctCount,
            MaxScore = maxScore,
            Percentage = percentage,
            IsPassed = passed,
            AttemptNumber = previousAttemptsCount + 1,
            AttemptedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow,
            Answers = answerRecords
        };

        _context.QuizAttempts.Add(attempt);

        if (usingGrant && unusedGrant != null)
        {
            unusedGrant.IsUsed = true;
            unusedGrant.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var remainingAttempts = Math.Max(0, allowedAttempts - attempt.AttemptNumber);

        // Offer the weak-points review only once: right when they've just burned their
        // last regular attempt (not the bonus one) and failed.
        var canRequestReview = !passed
            && remainingAttempts == 0
            && !usingGrant
            && unusedGrant == null;

        return new SubmitQuizResultDto
        {
            QuizAttemptId = attempt.Id,
            Score = correctCount,
            MaxScore = maxScore,
            Percentage = percentage,
            IsPassed = passed,
            AttemptNumber = attempt.AttemptNumber,
            MaxAttempts = quiz.MaxAttempts,
            RemainingAttempts = remainingAttempts,
            CanRequestWeakPointsReview = canRequestReview
        };
    }
}
