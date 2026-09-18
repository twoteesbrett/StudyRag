using StudyRag.Core.Services;
using StudyRag.Helpers;

namespace StudyRag.ConsoleApp;

public sealed class Chat(IndexingService indexing, QuestionAnsweringService answering)
{
    public async Task RunAsync(string dataDirectory)
    {
        Console.WriteLine("Loading documents...");

        var chunks = await indexing.IndexDirectoryAsync(dataDirectory);

        if (chunks.Count == 0)
        {
            Console.WriteLine("No document text was found. Answers will use general knowledge.");
        }

        while (true)
        {
            Console.Write("\nAsk a question (or type 'exit'): ");

            var question = Console.ReadLine();

            if (question is null || question.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                continue;
            }

            var answer = await answering.AnswerAsync(question, chunks);

            if (answer.Assessments.Any(result => !result.IsValid))
            {
                Console.WriteLine("Some document evidence could not be checked; the answer may be incomplete.");
            }

            if (answer.Sources.Count == 0 && answer.RelatedSources.Count > 0)
            {
                Console.WriteLine("No passages directly answered the question. Related passages were supplied to check its assumptions and missing details.");
            }
            else if (answer.Sources.Count == 0)
            {
                Console.WriteLine("No supporting passages were selected. Any general background is not evidence from your material.");
            }

            DebugConsole.WriteHeader("ANSWER");

            Console.WriteLine(answer.Text);
        }
    }
}
