using Microsoft.Extensions.AI;

namespace StudyRag.Services;

public class EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string text)
    {
        var embeddings = await _embeddingGenerator.GenerateAsync([text]);

        return embeddings[0].Vector;
    }
}
