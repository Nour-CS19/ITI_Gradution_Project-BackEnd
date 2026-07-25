using Femora.Application.Common.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;


public interface IAIQuizGeneratorRepository
{
    Task<AIGeneratedQuiz> GenerateQuizAsync(
        string topicTitle,
        string contextText,
        int questionCount,
        int choicesPerQuestion,
        CancellationToken cancellationToken = default);
}