using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.LMS.Quizzes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Commands.SubmitQuiz;

public class SubmitQuizCommandHandler(IAppDbContext db) : IRequestHandler<SubmitQuizCommand, SubmitQuizResponse>
{
    public async Task<SubmitQuizResponse> Handle(SubmitQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qst => qst.Choices)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz", request.QuizId.ToString());

        if (quiz.Questions.Count == 0)
            throw new InvalidOperationException("This quiz has no questions.");

        var previousAttemptsCount = await db.QuizAttempts
            .CountAsync(a => a.QuizId == quiz.Id && a.TraineeProfileId == request.TraineeProfileId, cancellationToken);

        if (previousAttemptsCount >= quiz.MaxAttempts)
            throw new InvalidOperationException("Maximum number of attempts reached for this quiz.");

        var questionsById = quiz.Questions.ToDictionary(q => q.Id);
        var maxScore = quiz.Questions.Count;

        var resultDtos = new List<SubmittedAnswerResultDto>();
        var attemptAnswers = new List<QuizAttemptAnswer>();
        var correctCount = 0;

        // Only consider one answer per question; ignore unknown/duplicate question ids.
        var answersByQuestion = request.Answers
            .GroupBy(a => a.QuestionId)
            .Select(g => g.First());

        foreach (var answer in answersByQuestion)
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question))
                continue;

            var selectedChoice = question.Choices.FirstOrDefault(c => c.Id == answer.ChoiceId);
            var correctChoice = question.Choices.FirstOrDefault(c => c.IsCorrect);

            if (selectedChoice is null || correctChoice is null)
                continue;

            var isCorrect = selectedChoice.IsCorrect;
            if (isCorrect) correctCount++;

            attemptAnswers.Add(new QuizAttemptAnswer
            {
                QuestionId = question.Id,
                ChoiceId = selectedChoice.Id,
                IsCorrect = isCorrect
            });

            resultDtos.Add(new SubmittedAnswerResultDto
            {
                QuestionId = question.Id,
                ChoiceId = selectedChoice.Id,
                IsCorrect = isCorrect,
                CorrectChoiceId = correctChoice.Id
            });
        }

        var score = maxScore == 0 ? 0 : Math.Round((decimal)correctCount / maxScore * 100, 2);
        var isPassed = score >= quiz.MinimumPassingScore;

        var attempt = new Domain.Entities.LMS.Quizzes.QuizAttempt
        {
            QuizId = quiz.Id,
            TraineeProfileId = request.TraineeProfileId,
            Score = score,
            MaxScore = maxScore,
            IsPassed = isPassed,
            AttemptNumber = previousAttemptsCount + 1,
            AttemptedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var attemptAnswer in attemptAnswers)
            attempt.Answers.Add(attemptAnswer);

        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        return new SubmitQuizResponse
        {
            QuizAttemptId = attempt.Id,
            Score = score,
            MaxScore = maxScore,
            IsPassed = isPassed,
            AttemptNumber = attempt.AttemptNumber,
            Results = resultDtos
        };
    }
}