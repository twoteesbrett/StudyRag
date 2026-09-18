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
    RagService generation)
{
    public async Task<QuestionAnswer> AnswerAsync(string question, IReadOnlyList<DocumentChunk> chunks)
    {
        var candidates = chunks.Count == 0
            ? []
            : await retrieval.FindBestMatchesAsync(question, chunks, count: 3, minSimilarity: 0.60f);

        var assessments = await assessment.AssessAsync(question, candidates);

        var selected = assessments
            .Where(result => result.IsSelected)
            .Select(result => result.Retrieval)
            .ToArray();

        IReadOnlyList<RetrievalResult> related = selected.Length == 0 ? candidates : [];

        var answer = await generation.AskAsync(question, selected, related);

        return new(answer, selected, related, assessments);
    }
}

public sealed record QuestionAnswer(
    string Text,
    IReadOnlyList<RetrievalResult> Sources,
    IReadOnlyList<RetrievalResult> RelatedSources,
    IReadOnlyList<EvidenceAssessment> Assessments);
