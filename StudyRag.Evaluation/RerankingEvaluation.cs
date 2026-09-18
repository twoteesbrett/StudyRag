using StudyRag.Core.Services;
using StudyRag.Core.Models;

namespace StudyRag.Evaluation;

public static class RerankingEvaluation
{
    public static async Task RunAsync(IndexingService indexing, RetrievalService retrieval,
        EvidenceReranker reranker, string dataDirectory)
    {
        await RunFixture("class-material.txt",
        [
            ("What healthcare provider does Moana want?", [5]),
            ("What is the name of Moana's preferred healthcare provider?", []),
            ("What health condition does Moana have, and what kind of healthcare provider does she want?", [5]),
            ("Compare Zara's and Tane's barriers to participating in healthcare. What specific support is suggested for each person?", [3, 4]),
            ("What exact dose of diabetes medication has Tane been prescribed?", [])
        ]);
        await RunFixture("overlap.txt",
        [
            ("Which service lifetime creates a new instance every time the service is requested?", [1]),
            ("How do scoped and singleton services differ in instance reuse across HTTP requests?", [2, 3]),
            ("How many bytes of memory does each scoped service instance use?", [])
        ]);

        async Task RunFixture(string file, (string Question, int[] Expected)[] cases)
        {
            var chunks = await indexing.IndexAsync(Path.Combine(dataDirectory, "Evaluation", file));
            var baseline = new Metrics();
            var reranked = new Metrics();
            int invalid = 0;
            Console.WriteLine($"\nFixture: {file}; same top-3 candidates at 0.60 for both methods.");
            foreach (var (question, expected) in cases)
            {
                Console.WriteLine($"\nQuestion: {question}");
                var candidates = await retrieval.FindBestMatchesAsync(question, chunks, count: 3, minSimilarity: 0.60f);
                var assessed = await reranker.RankAsync(question, candidates);
                invalid += assessed.Count(r => !r.IsValid);
                var selected = assessed.Where(r => r.IsSelected).Select(r => r.Retrieval).ToArray();
                baseline.Add(candidates, expected);
                reranked.Add(selected, expected, assessed.All(r => r.IsValid));
                foreach (var result in assessed)
                    Console.WriteLine($"  [{file}, chunk {result.Retrieval.Chunk.ChunkNumber}] similarity={result.Retrieval.Similarity:F3} support={result.Score}/3 selected={result.IsSelected}\n    Evidence: {result.Evidence}\n    Reason: {result.Reason}");
                Console.WriteLine($"  Expected [{string.Join(", ", expected)}]; selected [{string.Join(", ", selected.Select(r => r.Chunk.ChunkNumber))}]");
            }
            Console.WriteLine($"Baseline: {baseline}");
            Console.WriteLine($"Reranked: {reranked}");
            Console.WriteLine($"Invalid assessments: {invalid}; affected cases cannot count as exact or correctly unsupported.");
        }
        Console.WriteLine("Scores are ordinal model judgments, not confidence probabilities. Quotes verify provenance, not entailment. This evaluates passage selection, not generated answers.");
    }

    private sealed class Metrics
    {
        private int correct, returned, relevant, exact, total, unsupportedEmpty, unsupported;
        public void Add(IEnumerable<RetrievalResult> results, int[] expected, bool valid = true)
        {
            var actual = results.Select(r => r.Chunk.ChunkNumber).ToHashSet();
            correct += actual.Count(expected.Contains);
            returned += actual.Count;
            relevant += expected.Length;
            total++;
            if (valid && actual.SetEquals(expected)) exact++;
            if (expected.Length == 0)
            {
                unsupported++;
                if (valid && actual.Count == 0) unsupportedEmpty++;
            }
        }
        public override string ToString() =>
            $"precision {(returned == 0 ? "n/a" : ((double)correct / returned).ToString("P1"))}; recall {(relevant == 0 ? "n/a" : ((double)correct / relevant).ToString("P1"))}; exact {exact}/{total}; unsupported empty {unsupportedEmpty}/{unsupported}";
    }
}
