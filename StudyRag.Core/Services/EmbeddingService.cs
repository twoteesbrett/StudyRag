using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace StudyRag.Core.Services;

public class EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, ILogger<EmbeddingService>? logger = null)
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string text)
    {
        logger?.LogDebug("Requesting an embedding for {Length} characters.", text.Length);
        var timer = Stopwatch.StartNew();
        var embeddings = await _embeddingGenerator.GenerateAsync([text]);

        logger?.LogDebug("Embedding returned {Dimensions} dimensions in {Elapsed} ms.",
            embeddings[0].Vector.Length, timer.ElapsedMilliseconds);

        return embeddings[0].Vector;
    }
}
