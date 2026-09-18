using StudyRag.Core.Models;
using StudyRag.Core.Services;
using StudyRag.Helpers;

namespace StudyRag.ConsoleApp;

public sealed class ChatSession(
    IndexingService indexingService,
    RetrievalService retrievalService,
    EvidenceReranker reranker,
    RagService ragService)
{
    public async Task RunAsync(string dataDirectory, bool useReranker)
    {
        var chunks = await indexingService.IndexDirectoryAsync(dataDirectory);

        if (chunks.Count == 0)
        {
            Console.WriteLine("No document text was found. Answers will use general knowledge.");
        }

        while (true)
        {
            Console.Write("\nAsk a question (or type 'exit'): ");
            var question = Console.ReadLine();

            if (question is null ||
                question.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                continue;
            }

            IReadOnlyList<RetrievalResult> matches = chunks.Count == 0 ? [] : await retrievalService.FindBestMatchesAsync(
                question,
                chunks,
                count: 3,
                minSimilarity: 0.60f);

#if DEBUG
            DebugConsole.WriteHeader("RETRIEVED CHUNKS");
            DebugConsole.WriteLine($"Found {matches.Count} matching chunks.");

            foreach (var match in matches)
            {
                DebugConsole.WriteLine(
                    $"Source: {match.Chunk.SourceFile}, chunk {match.Chunk.ChunkNumber}");
                DebugConsole.WriteLine($"Similarity: {match.Similarity:F3}");
                DebugConsole.WriteLine(match.Chunk.Text);
                DebugConsole.WriteLine("");
            }
#endif

            if (useReranker && matches.Count > 0)
            {
                var assessed = await reranker.RankAsync(question, matches);
                foreach (var result in assessed)
                    Console.WriteLine($"[{result.Retrieval.Chunk.SourceFile}, chunk {result.Retrieval.Chunk.ChunkNumber}] Support: {result.Score}/3 - {result.Reason}");
                if (assessed.Any(r => !r.IsValid))
                {
                    Console.WriteLine("Some passages could not be assessed and will be excluded from the document context.");
                }
                matches = assessed.Where(r => r.IsSelected).Select(r => r.Retrieval).ToArray();
            }

            if (matches.Count == 0)
                Console.WriteLine("No document passages were selected. The answer can provide general background, but cannot establish missing details from your material.");

            var answer = await ragService.AskAsync(question, matches);

            Console.WriteLine($"\nAnswer:\n{answer}");
        }

    }
}