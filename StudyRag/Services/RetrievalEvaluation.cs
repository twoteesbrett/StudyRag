using StudyRag.Models;

namespace StudyRag.Services;

public static class RetrievalEvaluation
{
    // Expected chunk numbers refer to the paragraphs in Data/sample1.txt.
    private static readonly (string Question, int[] Expected)[] Cases =
    [
        ("Why do people keep dogs as companions?", [1]),
        ("How do indexes help SQL Server queries?", [2]),
        ("How does dependency injection make code easier to test?", [3]),
        ("How long does the Earth take to orbit the Sun?", [4]),
        ("What causes the seasons on Earth?", [4]),
        ("How does RAG use retrieved documents to answer questions?", [5]),
        ("How are embeddings used to find relevant text?", [6]),
        ("How do I bake a chocolate cake?", []),
        ("Who won the football World Cup in 2022?", []),
        ("What is the population of Tokyo?", [])
    ];

    public static async Task RunAsync(RetrievalService retrievalService, IReadOnlyList<DocumentChunk> chunks)
    {
        float[] thresholds = [0.40f, 0.45f, 0.50f, 0.55f, 0.60f, 0.65f, 0.70f];
        var measurements = new List<(int[] Expected, IReadOnlyList<RetrievalResult> Ranked)>();
        foreach (var (question, expected) in Cases)
        {
            var ranked = await retrievalService.FindBestMatchesAsync(question, chunks, count: chunks.Count);
            measurements.Add((expected, ranked));
            Console.WriteLine($"\nQuestion: {question}");
            Console.WriteLine($"Expected chunks: {(expected.Length == 0 ? "none" : string.Join(", ", expected))}");
            foreach (var result in ranked)
            {
                var decision = result.Similarity >= 0.60f ? "accepted" : "rejected";
                Console.WriteLine($"  Chunk {result.Chunk.ChunkNumber}: {result.Similarity:F3} ({decision} at 0.60)");
            }
        }

        Console.WriteLine("\nThreshold comparison (top 3 after filtering):");
        Console.WriteLine("Cutoff  Precision  Recall  Exact cases  Unrelated correctly empty");
        foreach (var threshold in thresholds)
        {
            int correct = 0, returned = 0, relevant = 0, exact = 0, unrelatedEmpty = 0, unrelated = 0;
            foreach (var (expected, ranked) in measurements)
            {
                var actual = ranked.Where(r => r.Similarity >= threshold).Take(3)
                    .Select(r => r.Chunk.ChunkNumber).ToHashSet();
                correct += actual.Count(expected.Contains);
                returned += actual.Count;
                relevant += expected.Length;
                if (actual.SetEquals(expected)) exact++;
                if (expected.Length == 0)
                {
                    unrelated++;
                    if (actual.Count == 0) unrelatedEmpty++;
                }
            }
            var precision = returned == 0 ? "n/a" : $"{(double)correct / returned:P1}";
            var recall = relevant == 0 ? "n/a" : $"{(double)correct / relevant:P1}";
            Console.WriteLine($"{threshold:F2}    {precision,9}  {recall,6}  {exact}/{Cases.Length}           {unrelatedEmpty}/{unrelated}");
        }
        Console.WriteLine("\nSimilarity is not a confidence percentage. Validate cutoff changes on additional questions.");
    }
}
