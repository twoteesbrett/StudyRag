using StudyRag.Models;

namespace StudyRag.Services;

public class IndexingService(
    TextFileLoader fileLoader,
    EmbeddingService embeddingService)
{
    public async Task<IReadOnlyList<DocumentChunk>> IndexDirectoryAsync(string directoryPath)
    {
        var paths = Directory.EnumerateFiles(directoryPath, "*.txt")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
            
        var chunks = new List<DocumentChunk>();

        foreach (var path in paths)
        {
            chunks.AddRange(await IndexAsync(path));
        }

        return chunks;
    }

    public async Task<IReadOnlyList<DocumentChunk>> IndexAsync(
        string path)
    {
        var text = await fileLoader.LoadAsync(path);

        var documentTexts = TextChunker.Chunk(text);

        var chunks = new List<DocumentChunk>();

        for (var index = 0; index < documentTexts.Count; index++)
        {
            var documentText = documentTexts[index];
            var embedding =
                await embeddingService.GenerateAsync(documentText);

            chunks.Add(new DocumentChunk
            {
                SourceFile = Path.GetFileName(path),
                ChunkNumber = index + 1,
                Text = documentText,
                Embedding = embedding
            });
        }

        return chunks;
    }
}
