namespace StudyRag.Models;

public record DocumentChunk
{
    public required string Text { get; init; }

    public required ReadOnlyMemory<float> Embedding { get; init; }
}