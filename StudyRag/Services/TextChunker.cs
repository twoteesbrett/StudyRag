namespace StudyRag.Services;

public class TextChunker
{
    public IReadOnlyList<string> Chunk(
        string text,
        int chunkSize = 500)
    {
        var chunks = new List<string>();

        for (var i = 0; i < text.Length; i += chunkSize)
        {
            var length = Math.Min(
                chunkSize,
                text.Length - i);

            var chunk = text.Substring(i, length);

            chunks.Add(chunk);
        }

        return chunks;
    }
}