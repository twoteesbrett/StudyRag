namespace StudyRag.Core.Services;

public static class TextChunker
{
    public static IReadOnlyList<string> Chunk(
        string text,
        int maxCharacters = 1000,
        int overlapCharacters = 100)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCharacters, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(overlapCharacters);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(overlapCharacters, maxCharacters);

        var paragraphs = text.Split(
            ["\r\n\r\n", "\n\n"],
            StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            var start = 0;

            while (start < trimmed.Length)
            {
                var end = start + Math.Min(maxCharacters, trimmed.Length - start);

                if (end < trimmed.Length)
                {
                    // Keep enough new text to advance even when overlap is large.
                    var minimumEnd = start + Math.Max(maxCharacters / 2, overlapCharacters + 1);
                    end = FindSplit(trimmed, minimumEnd, end);
                }

                var chunk = trimmed[start..end].Trim();
                if (chunk.Length > 0)
                    chunks.Add(chunk);

                if (end == trimmed.Length)
                    break;

                start = end - overlapCharacters;
            }
        }

        return chunks;
    }

    private static int FindSplit(string text, int minimumEnd, int maximumEnd)
    {
        // Prefer a sentence ending, then whitespace, then a hard character limit.
        for (var end = maximumEnd; end >= minimumEnd; end--)
        {
            if (text[end - 1] is '.' or '!' or '?' && char.IsWhiteSpace(text[end]))
                return end;
        }

        for (var end = maximumEnd; end >= minimumEnd; end--)
        {
            if (char.IsWhiteSpace(text[end]))
                return end;
        }

        return maximumEnd;
    }
}
