using StudyRag.Core.Models;
using StudyRag.Core.Services;
using Microsoft.Extensions.Logging;

public sealed class Chat(QuestionAnsweringService answering, ILogger<Chat> logger)
{
    public async Task RunAsync(IReadOnlyList<DocumentChunk> chunks)
    {
        if (chunks.Count == 0)
        {
            logger.LogWarning("No document text was found; answers have no document evidence.");
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Chat ready with {Count} indexed chunks.", chunks.Count);
        }

        while (true)
        {
            Console.Write("\nAsk a question (or type 'exit'): ");

            var question = Console.ReadLine();

            if (question is null || question.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Chat ended.");
                break;
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                continue;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Answering a question of {Length} characters.", question.Length);
            }

            QuestionAnswer answer;

            try
            {
                answer = await answering.AnswerAsync(question, chunks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not answer the question. Enter another question to try again.");
                continue;
            }

            if (answer.Assessments.Any(result => !result.IsValid))
            {
                logger.LogWarning("Some document evidence could not be checked; the answer may be incomplete.");
            }

            if (answer.Sources.Count == 0 && answer.RelatedSources.Count > 0)
            {
                logger.LogInformation("No passages directly answered the question. Related passages were supplied to check its assumptions and missing details.");
            }
            else if (answer.Sources.Count == 0)
            {
                logger.LogWarning("No supporting passages were selected. Any general background is not evidence from your material.");
            }

            Console.WriteLine("\n=== ANSWER ===");

            Console.WriteLine(answer.Text);
        }
    }
}
