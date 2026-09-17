using StudyRag.Models;

namespace StudyRag.Services;

public class RetrievalService(EmbeddingService embeddingService)
{
    public async Task<IReadOnlyList<RetrievalResult>> FindBestMatchesAsync(
        string question,
        IEnumerable<DocumentChunk> chunks,
        int count = 3,
        float? minSimilarity = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, nameof(count));

        var questionEmbedding =
            await embeddingService.GenerateAsync(question);

        var results = chunks
            .Select(chunk => new RetrievalResult(
                chunk,
                VectorMath.CosineSimilarity(
                    questionEmbedding.Span,
                    chunk.Embedding.Span)))
            .Where(result =>
                minSimilarity is null ||
                result.Similarity >= minSimilarity.Value)
            .OrderByDescending(result => result.Similarity)
            .Take(count)
            .ToList();

        return results;
    }
}
