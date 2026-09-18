namespace StudyRag.Core.Models;

// Ordinal evidence strength, not a calibrated probability.
public record EvidenceAssessment(RetrievalResult Retrieval, int Score, string Evidence, string Reason, bool IsValid = true)
{
    public bool IsSelected => IsValid && Score >= 2;
}
