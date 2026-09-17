namespace StudyRag.Services;

public class TextChunker
{
    public static IReadOnlyList<string> Chunk(string text) => [.. text
        .Split(
            ["\r\n\r\n", "\n\n"],
            StringSplitOptions.RemoveEmptyEntries)
        .Select(chunk => chunk.Trim())
        .Where(chunk => !string.IsNullOrWhiteSpace(chunk))];
}