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
            logger.LogInformation("Finding matching passages...");
        }

        logger?.LogDebug("Retrieval limit {Count}; minimum similarity {Minimum}.", count, minSimilarity);

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
            logger.LogInformation("Found {Count} matching passages.", results.Count);
        }

        if (logger?.IsEnabled(LogLevel.Debug) == true)
        {
            foreach (var result in results)
            {
                logger.LogDebug("[{Source}, chunk {Chunk}] Similarity: {Similarity:F3}; {Length} characters.",
                    result.Chunk.SourceFile, result.Chunk.ChunkNumber, result.Similarity, result.Chunk.Text.Length);
                logger.LogTrace("Retrieved text: {Text}", result.Chunk.Text);
            }
        }

        return results;
    }
}
