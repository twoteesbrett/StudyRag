namespace StudyRag.Models;

public record RetrievalResult(
    DocumentChunk Chunk,
    float Similarity);