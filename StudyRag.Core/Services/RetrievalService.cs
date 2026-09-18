using StudyRag.Core.Logging;
using Microsoft.Extensions.Logging;
using StudyRag.Core.Models;
using StudyRag.Core.Helpers;

namespace StudyRag.Core.Services;

public class RetrievalService(EmbeddingService embeddingService, ILogger<RetrievalService>? logger = null)
{
    public async Task<IReadOnlyList<RetrievalResult>> FindBestMatchesAsync(
        string question,
        IEnumerable<DocumentChunk> chunks,
        int count = 3,
        float? minSimilarity = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, nameof(count));

        if (logger?.IsEnabled(LogLevel.Information) == true)
        {
            logger.LogInformation(LogEvents.Action, "Finding matching passages...");
        }

        var questionEmbedding =
            await embeddingService.GenerateAsync(question);

        var results = chunks
            .Select(chunk => new RetrievalResult(chunk, VectorMath.CosineSimilarity(questionEmbedding.Span, chunk.Embedding.Span)))
            .Where(result => minSimilarity is null || result.Similarity >= minSimilarity.Value)
            .OrderByDescending(result => result.Similarity)
            .Take(count)
            .ToList();

        if (logger?.IsEnabled(LogLevel.Information) == true)
        {
            logger.LogInformation(LogEvents.Response, "Found {Count} matching passages.", results.Count);
        }

        if (logger?.IsEnabled(LogLevel.Debug) == true)
        {
            foreach (var result in results)
            {
                logger.LogDebug(LogEvents.Response, "[{Source}, chunk {Chunk}] Similarity: {Similarity:F3}\n{Text}",
                    result.Chunk.SourceFile, result.Chunk.ChunkNumber, result.Similarity, result.Chunk.Text);
            }
        }

        return results;
    }
}
