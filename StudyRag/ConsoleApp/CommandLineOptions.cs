namespace StudyRag.ConsoleApp;

public enum AppMode
{
    Chat,
    SampleRetrieval,
    Precision,
    SyntheticOverlap,
    Reranking
}

public sealed record CommandLineOptions(AppMode Mode, bool UseReranker = false)
{
    public static bool TryParse(string[] args, out CommandLineOptions options)
    {
        options = new(AppMode.Chat);
        if (args.Length == 0) return true;
        if (args.Length != 1) return false;

        var parsed = args[0] switch
        {
            "--rerank" => new CommandLineOptions(AppMode.Chat, true),
            "--evaluate-sample-retrieval" => new(AppMode.SampleRetrieval),
            "--evaluate-precision" => new(AppMode.Precision),
            "--evaluate-synthetic-overlap" => new(AppMode.SyntheticOverlap),
            "--evaluate-reranking" => new(AppMode.Reranking),
            _ => null
        };
        if (parsed is null) return false;
        options = parsed;
        return true;
    }

    public const string Help = """
        Usage: dotnet run --project StudyRag -- [option]
          (no option)                    Ask questions using similarity retrieval.
          --rerank                       Ask questions with evidence-support reranking.
          --evaluate-sample-retrieval     Compare retrieval cutoffs on 33 sample1.txt questions.
          --evaluate-precision            Measure retrieval precision on your overlapping class passages.
          --evaluate-synthetic-overlap    Measure retrieval on synthetic service-lifetime passages.
          --evaluate-reranking            Compare baseline and reranked precision/recall on both overlap fixtures.
          --help                         Show this help.
        Evaluation modes measure passage selection, not generated-answer accuracy.
        Choose one evaluation mode. --rerank applies only to interactive questions.
        """;
}
