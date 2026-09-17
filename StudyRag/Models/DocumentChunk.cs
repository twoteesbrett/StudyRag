namespace StudyRag.Models;

public record DocumentChunk
{
    public required string SourceFile { get; init; }

    public required int ChunkNumber { get; init; }

    public required string Text { get; init; }

    public required ReadOnlyMemory<float> Embedding { get; init; }
}
