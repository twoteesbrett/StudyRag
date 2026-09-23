namespace StudyRag.Core.Models;

/// <summary>A proposed chunk boundary using inclusive, one-based block IDs.</summary>
public record ChunkRange(int Start, int End);
