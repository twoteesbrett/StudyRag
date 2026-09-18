using StudyRag.Core.Services;
using StudyRag.Core.Models;

namespace StudyRag.Evaluation;

public static class RetrievalEvaluation
{
    // Expected numbers refer to sample1.txt paragraphs that contain the answer.
    // Sharing a topic alone does not count as answer support.
    private static readonly (string Question, int[] Expected)[] Cases =
    [
        ("Why do people keep dogs as companions?", [1]),
        ("How do indexes help SQL Server queries?", [2]),
        ("How does dependency injection make code easier to test?", [3]),
        ("How long does the Earth take to orbit the Sun?", [4]),
        ("What causes the seasons on Earth?", [4]),
        ("tell me about the earth and sun", [4]),
        ("How does RAG use retrieved documents to answer questions?", [5]),
        ("How are embeddings used to find relevant text?", [6]),
        ("Why are dogs good companions for humans?", [1]),
        ("What helps SQL Server find rows faster?", [2]),
        ("Why does receiving dependencies instead of constructing them improve testability?", [3]),
        ("How many days are in one trip of Earth around the Sun?", [4]),
        ("How long is a year on Earth?", [4]),
        ("What shape is Earth's path around the Sun?", [4]),
        ("Why do seasons change?", [4]),
        ("Does Earth's tilt cause the seasons?", [4]),
        ("How can a language model use information outside its training data?", [5]),
        ("Why can vectors help search for text with similar meaning?", [6]),
        ("How do I bake a chocolate cake?", []),
        ("Who won the football World Cup in 2022?", []),
        ("What is the population of Tokyo?", [])
    ];

    private static readonly string[] UnsupportedQuestions =
    [
        "What is the average lifespan of a dog?",
        "Which dog breed needs the most exercise?",
        "What SQL command creates an index in SQL Server?",
        "What is the difference between clustered and nonclustered indexes in SQL Server?",
        "What is the difference between singleton and scoped dependency injection lifetimes?",
        "How do I register a service with .NET dependency injection?",
        "How far is the Earth from the Sun in kilometres?",
        "What is the angle of Earth's axial tilt in degrees?",
        "What is Earth's orbital speed around the Sun?",
        "What chunk size should I use in a RAG system?",
        "Which embedding model performs best for RAG?",
        "How many dimensions do the text embeddings have?"
    ];

    public static async Task RunAsync(RetrievalService retrievalService, IReadOnlyList<DocumentChunk> chunks)
    {

        var cases = Cases.Select(c => (c.Question, c.Expected,
                Category: c.Expected.Length == 0 ? "unrelated" : "answerable"))
            .Concat(UnsupportedQuestions.Select(q =>
                (Question: q, Expected: Array.Empty<int>(), Category: "unsupported")))
            .ToArray();
        await RunCasesAsync(retrievalService, chunks, cases);
    }

    public static async Task RunOverlapAsync(RetrievalService retrievalService, IReadOnlyList<DocumentChunk> chunks)
    {
        Console.WriteLine("Synthetic overlap baseline: four service-lifetime paragraphs, not actual class notes.");
        // Paragraph labels are specific to Data/Evaluation/overlap.txt.
        // Both paragraphs 2 and 3 are needed for the comparison question.
        await RunCasesAsync(retrievalService, chunks,
        [
            ("Which service lifetime creates a new instance every time the service is requested?", [1], "answerable"),
            ("How do scoped and singleton services differ in instance reuse across HTTP requests?", [2, 3], "answerable"),
            ("How many bytes of memory does each scoped service instance use?", [], "unsupported")
        ]);
    }

    public static async Task RunClassMaterialAsync(RetrievalService retrievalService, IReadOnlyList<DocumentChunk> chunks)
    {
        Console.WriteLine("Class-material baseline: five verbatim passages from the supplied test corpus.");
        Console.WriteLine("Source mapping and label rationale: evaluation/class-material.md");
        // Labels refer to the selected passages in Data/Evaluation/class-material.txt.
        // The comparison needs both case-specific passages, not generic advice.
        await RunCasesAsync(retrievalService, chunks,
        [
            ("What health condition does Moana have, and what kind of healthcare provider does she want?", [5], "answerable"),
            ("Compare Zara's and Tane's barriers to participating in healthcare. What specific support is suggested for each person?", [3, 4], "answerable"),
            ("What exact dose of diabetes medication has Tane been prescribed?", [], "unsupported")
        ]);
    }

    private static async Task RunCasesAsync(
        RetrievalService retrievalService,
        IReadOnlyList<DocumentChunk> chunks,
        (string Question, int[] Expected, string Category)[] cases)
    {
        float[] thresholds = [0.40f, 0.45f, 0.50f, 0.55f, 0.60f, 0.65f, 0.70f];
        var measurements = new List<(string Question, string Category, int[] Expected, IReadOnlyList<RetrievalResult> Ranked)>();
        foreach (var (question, expected, category) in cases)
        {
            var ranked = await retrievalService.FindBestMatchesAsync(question, chunks, count: chunks.Count);
            measurements.Add((question, category, expected, ranked));
            Console.WriteLine($"\nQuestion ({category}): {question}");
            Console.WriteLine($"Expected chunks: {(expected.Length == 0 ? "none" : string.Join(", ", expected))}");
            foreach (var result in ranked)
            {
                var decision = result.Similarity >= 0.60f ? "passes cutoff" : "below cutoff";
                Console.WriteLine($"  Chunk {result.Chunk.ChunkNumber}: {result.Similarity:F3} ({decision} at 0.60)");
            }
        }

        Console.WriteLine("\nThreshold comparison (top 3 after filtering):");
        Console.WriteLine("Cutoff  Precision  Recall  Exact cases  Unrelated empty  Unsupported empty");
        foreach (var threshold in thresholds)
        {
            int correct = 0, returned = 0, relevant = 0, exact = 0, unrelatedEmpty = 0, unrelated = 0;
            int unsupportedEmpty = 0, unsupported = 0;
            foreach (var (_, category, expected, ranked) in measurements)
            {
                var actual = ranked.Where(r => r.Similarity >= threshold).Take(3)
                    .Select(r => r.Chunk.ChunkNumber).ToHashSet();
                correct += actual.Count(expected.Contains);
                returned += actual.Count;
                relevant += expected.Length;
                if (actual.SetEquals(expected)) exact++;
                if (category == "unrelated")
                {
                    unrelated++;
                    if (actual.Count == 0) unrelatedEmpty++;
                }
                if (category == "unsupported")
                {
                    unsupported++;
                    if (actual.Count == 0) unsupportedEmpty++;
                }
            }
            var precision = returned == 0 ? "n/a" : $"{(double)correct / returned:P1}";
            var recall = relevant == 0 ? "n/a" : $"{(double)correct / relevant:P1}";
            Console.WriteLine($"{threshold:F2}    {precision,9}  {recall,6}  {exact}/{cases.Length}           {unrelatedEmpty}/{unrelated}              {unsupportedEmpty}/{unsupported}");
        }
        foreach (var threshold in new[] { 0.60f, 0.65f })
        {
            Console.WriteLine($"\nFailures at {threshold:F2}:");
            foreach (var (question, category, expected, ranked) in measurements)
            {
                var actual = ranked.Where(r => r.Similarity >= threshold).Take(3)
                    .Select(r => r.Chunk.ChunkNumber).ToHashSet();
                if (!actual.SetEquals(expected))
                    Console.WriteLine($"  [{category}] {question} Expected [{string.Join(", ", expected)}]; got [{string.Join(", ", actual)}].");
            }
        }
        Console.WriteLine("\nExpected chunks must contain the answer, not merely share the question's topic.");
        Console.WriteLine("Similarity is not a confidence percentage. Validate cutoff changes on additional questions.");
    }
}
