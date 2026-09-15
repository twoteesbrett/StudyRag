using StudyRag.Models;
using StudyRag.Services;

namespace StudyRag.Services;

public class RetrievalService(EmbeddingService embeddingService)
{
    public async Task<IReadOnlyList<RetrievalResult>> FindBestMatchesAsync(
        string question,
        IEnumerable<DocumentChunk> chunks,
        int count = 3)
    {
        var questionEmbedding =
            await embeddingService.GenerateAsync(question);

        var results = chunks
            .Select(chunk => new RetrievalResult(
                chunk,
                VectorMath.CosineSimilarity(
                    questionEmbedding.Span,
                    chunk.Embedding.Span)))
            .OrderByDescending(result => result.Similarity)
            .Take(count)
            .ToList();

        return results;
    }
}
