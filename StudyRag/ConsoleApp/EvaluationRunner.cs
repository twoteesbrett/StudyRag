using StudyRag.Core.Services;
using StudyRag.Evaluation;

namespace StudyRag.ConsoleApp;

public sealed class EvaluationRunner(
    IndexingService indexing,
    RetrievalService retrieval,
    EvidenceReranker reranker)
{
    public async Task RunAsync(AppMode mode, string dataDirectory)
    {
        if (mode == AppMode.Reranking)
        {
            await RerankingEvaluation.RunAsync(indexing, retrieval, reranker, dataDirectory);
            return;
        }

        var file = mode switch
        {
            AppMode.SampleRetrieval => "sample1.txt",
            AppMode.Precision => Path.Combine("Evaluation", "class-material.txt"),
            AppMode.SyntheticOverlap => Path.Combine("Evaluation", "overlap.txt"),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Expected an evaluation mode.")
        };
        var chunks = await indexing.IndexAsync(Path.Combine(dataDirectory, file));
        await (mode switch
        {
            AppMode.SampleRetrieval => RetrievalEvaluation.RunAsync(retrieval, chunks),
            AppMode.Precision => RetrievalEvaluation.RunClassMaterialAsync(retrieval, chunks),
            _ => RetrievalEvaluation.RunOverlapAsync(retrieval, chunks)
        });
    }
}
