namespace StudyRag.Core.Models;

public record RetrievalResult(
    DocumentChunk Chunk,
    float Similarity);