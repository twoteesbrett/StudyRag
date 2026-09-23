using Microsoft.Extensions.Logging;
using StudyRag.Core.Models;

namespace StudyRag.Core.Services;

/// <summary>
/// Coordinates the question-answering process by retrieving relevant
/// passages, assessing their supporting evidence, and generating an answer.
/// Returns the answer along with its sources and evidence assessments.
/// </summary>
public sealed class QuestionAnsweringService(
    RetrievalService retrieval,
    EvidenceAssessmentService assessment,
    RagService generation,
    ILogger<QuestionAnsweringService>? logger = null)
{
    private const int RetrievalLimit = 10;

    public async Task<QuestionAnswer> AnswerAsync(string question, IReadOnlyList<DocumentChunk> chunks)
    {
        logger?.LogDebug("Question has {Length} characters; corpus contains {Count} chunks.", question.Length, chunks.Count);

        var candidates = chunks.Count == 0
            ? []
            : await retrieval.FindBestMatchesAsync(question, chunks, count: RetrievalLimit, minSimilarity: 0.60f);

        var assessments = await assessment.AssessAsync(question, candidates);

        var selected = assessments
            .Where(result => result.IsSelected)
            .Select(result => result.Retrieval)
            .ToArray();

        IReadOnlyList<RetrievalResult> related = selected.Length == 0 ? candidates : [];

        logger?.LogDebug("Evidence selection: {Candidates} candidates, {Selected} supporting, {Related} related.",
            candidates.Count, selected.Length, related.Count);

        var answer = await generation.AskAsync(question, selected, related);

        return new(answer, selected, related, assessments);
    }
}

public sealed record QuestionAnswer(
    string Text,
    IReadOnlyList<RetrievalResult> Sources,
    IReadOnlyList<RetrievalResult> RelatedSources,
    IReadOnlyList<EvidenceAssessment> Assessments);
