using Microsoft.Extensions.AI;
using StudyRag.Core.Models;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class RetrievalServiceTests
{
    [Fact]
    public async Task FindBestMatchesAsync_OrdersBySimilarity()
    {
        using var embeddings = new StubEmbeddings([1, 0]);
        var service = new RetrievalService(new EmbeddingService(embeddings));

        var results = await service.FindBestMatchesAsync("question", [
            Chunk(1, [0, 1]),
            Chunk(2, [1, 1]),
            Chunk(3, [1, 0])
        ], count: 3);

        Assert.Equal(new[] { 3, 2, 1 }, results.Select(result => result.Chunk.ChunkNumber));
    }

    [Fact]
    public async Task FindBestMatchesAsync_AppliesSimilarityThresholdBeforeLimit()
    {
        using var embeddings = new StubEmbeddings([1, 0]);
        var service = new RetrievalService(new EmbeddingService(embeddings));

        var results = await service.FindBestMatchesAsync("question", [
            Chunk(1, [0, 1]),
            Chunk(2, [1, 0]),
            Chunk(3, [1, 1])
        ], count: 2, minSimilarity: 0.70f);

        Assert.Equal(new[] { 2, 3 }, results.Select(result => result.Chunk.ChunkNumber));
    }

    [Fact]
    public async Task FindBestMatchesAsync_ReturnsCandidatesBeyondSmallLimitWhenRequested()
    {
        using var embeddings = new StubEmbeddings([1, 0]);
        var service = new RetrievalService(new EmbeddingService(embeddings));
        var chunks = Enumerable.Range(1, 12)
            .Select(number => Chunk(number, number == 12 ? [1, 0] : [0, 1]))
            .ToArray();

        var results = await service.FindBestMatchesAsync("question", chunks, count: 12);

        Assert.Contains(results, result => result.Chunk.ChunkNumber == 12);
    }

    [Fact]
    public async Task FindBestMatchesAsync_EmptyCorpusReturnsNoResults()
    {
        using var embeddings = new StubEmbeddings([1, 0]);
        var service = new RetrievalService(new EmbeddingService(embeddings));

        var results = await service.FindBestMatchesAsync("question", []);

        Assert.Empty(results);
    }

    private static DocumentChunk Chunk(int number, float[] embedding) => new()
    {
        SourceFile = "test.txt",
        ChunkNumber = number,
        Text = $"Chunk {number}",
        Embedding = embedding
    };

    private sealed class StubEmbeddings(float[] vector) : IEmbeddingGenerator<string, Embedding<float>>
    {
        public int Calls { get; private set; }

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            var result = new GeneratedEmbeddings<Embedding<float>>();
            foreach (var _ in values)
                result.Add(new Embedding<float>(vector));
            return Task.FromResult(result);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
