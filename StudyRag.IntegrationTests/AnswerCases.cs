namespace StudyRag.IntegrationTests;

internal sealed record AnswerCase
{
    public required string Question { get; init; }
    public required string ExpectedAnswer { get; init; }
    public string[] RequiredSources { get; init; } = [];
}

internal static class AnswerCases
{
    public static IReadOnlyDictionary<string, AnswerCase> All { get; } = new Dictionary<string, AnswerCase>
    {
        ["Direct fact"] = new()
        {
            Question = "What health condition does Moana have?",
            ExpectedAnswer = """
                States chronic asthma, supported by a citation.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Precision"] = new()
        {
            Question = "What kind of healthcare provider does Moana want?",
            ExpectedAnswer = """
                A provider incorporating Pasifika health models; generic cultural advice alone is insufficient.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Missing detail"] = new()
        {
            Question = "What is the name of Moana’s preferred healthcare provider?",
            ExpectedAnswer = """
                Explicitly says the provider name is not supplied. Does not invent or suggest an organisation as
                her preferred provider.
                """,
            RequiredSources = []
        },
        ["Overlapping topics"] = new()
        {
            Question = "Why does Tane struggle to attend check-ups?",
            ExpectedAnswer = """
                Explains remote rural location and limited transport. Does not assign Zara's speech or language
                difficulties to Tane.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Multiple passages"] = new()
        {
            Question = "Compare Zara’s and Tane’s barriers to healthcare and the support suggested for each.",
            ExpectedAnswer = """
                Zara: illness affects speech, English is an additional language, husband often answers for her;
                address her directly, give time and accessible communication aids or an appropriate interpreter.
                Tane: remote rural location and limited transport hinder diabetes check-ups; help with transport
                or appointment arrangements and contacting the healthcare team. Covers both accurately with
                cited evidence for EACH person's barriers and support; generic advice alone is insufficient.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Partially answerable"] = new()
        {
            Question = "What condition does Moana have, and what medication dose does she take?",
            ExpectedAnswer = """
                States chronic asthma and explicitly acknowledges no medication dose is supplied. Invents no
                drug or dose.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Incorrect premise"] = new()
        {
            Question = "Why does Moana need help managing her diabetes?",
            ExpectedAnswer = """
                Clearly corrects the premise: Moana has asthma; diabetes belongs to Tane. Provides citations
                supporting the correction, with no suggestion Moana has diabetes.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Similar concepts"] = new()
        {
            Question = "How is checking understanding different from simply asking “Do you understand?”",
            ExpectedAnswer = """
                Explains an open question asking the person to describe next steps in their own words, which
                reveals misunderstandings a yes/no answer may hide; clarify or rephrase if needed.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Unsupported inference"] = new()
        {
            Question = "Does Zara’s husband answering for her prove she cannot make decisions?",
            ExpectedAnswer = """
                Says no: husband answering does not establish incapacity. Distinguishes stated speech/language
                communication barriers from decision-making ability; does not diagnose incapacity.
                """,
            RequiredSources = ["precision.txt"]
        },
        ["Cross-topic retrieval"] = new()
        {
            Question = "How do dependency injection and test doubles help with unit testing?",
            ExpectedAnswer = """
                Explains receiving dependencies instead of constructing them enables substitutes/test doubles,
                isolating behaviour and making tests deterministic without external services/network access.
                Combines and cites relevant evidence from BOTH sample1.txt (dependency injection) and
                sample3.txt (test doubles).
                """,
            RequiredSources = ["sample1.txt", "sample3.txt"]
        },
        ["Basic sanity check"] = new()
        {
            Question = "What is the difference between a queue and a stack?",
            ExpectedAnswer = """
                Queue is first-in first-out (FIFO); stack is last-in first-out (LIFO), supported by sample3.txt.
                """,
            RequiredSources = ["sample3.txt"]
        }
    };
}

