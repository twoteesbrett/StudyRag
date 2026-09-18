using Microsoft.Extensions.AI;
using StudyRag.Core.Helpers;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class IndexingServiceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "StudyRagTests-" + Guid.NewGuid());

    public IndexingServiceTests() => Directory.CreateDirectory(directory);

    [Fact]
    public async Task IndexDirectoryAsync_CombinesTextFilesAndPreservesSources()
    {
        await File.WriteAllTextAsync(Path.Combine(directory, "b.txt"), "Third paragraph.");
        await File.WriteAllTextAsync(Path.Combine(directory, "a.txt"), "First paragraph.\n\nSecond paragraph.");
        await File.WriteAllTextAsync(Path.Combine(directory, "ignored.md"), "Ignore this.");
        await File.WriteAllTextAsync(Path.Combine(directory, "empty.txt"), "  ");
        var generator = new StubEmbeddingGenerator();
        var service = new IndexingService(new TextFileLoader(), new EmbeddingService(generator));

        var chunks = await service.IndexDirectoryAsync(directory);

        Assert.Equal(new[] { "a.txt", "a.txt", "b.txt" }, chunks.Select(c => c.SourceFile));
        Assert.Equal(new[] { 1, 2, 1 }, chunks.Select(c => c.ChunkNumber));
        Assert.Equal(new[] { "First paragraph.", "Second paragraph.", "Third paragraph." }, chunks.Select(c => c.Text));
        Assert.Equal(chunks.Select(c => c.Text), generator.Inputs);
        Assert.All(chunks, c => Assert.Equal(new float[] { 1, 0 }, c.Embedding.ToArray()));
    }

    [Fact]
    public async Task IndexDirectoryAsync_EmptyDirectory_ReturnsNoChunks()
    {
        var generator = new StubEmbeddingGenerator();
        var service = new IndexingService(new TextFileLoader(), new EmbeddingService(generator));

        Assert.Empty(await service.IndexDirectoryAsync(directory));
        Assert.Empty(generator.Inputs);
    }

    public void Dispose()
    {
        foreach (var path in Directory.EnumerateFiles(directory)) File.Delete(path);
        Directory.Delete(directory);
    }

    private sealed class StubEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public List<string> Inputs { get; } = [];

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values, EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var result = new GeneratedEmbeddings<Embedding<float>>();
            foreach (var value in values)
            {
                Inputs.Add(value);
                result.Add(new Embedding<float>(new float[] { 1, 0 }));
            }
            return Task.FromResult(result);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
