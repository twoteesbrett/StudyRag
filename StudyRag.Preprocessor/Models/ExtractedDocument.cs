namespace StudyRag.Preprocessor.Models;

/// <summary>Source content in reading order, before normalisation or chunking.</summary>
public record ExtractedDocument
{
    public required string SourceFile { get; init; }

    public string? Title { get; init; }

    public Uri? SourceUrl { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();

    public required IReadOnlyList<DocumentBlock> Blocks { get; init; }

    public IReadOnlyList<string> Warnings { get; init; } = [];
}
