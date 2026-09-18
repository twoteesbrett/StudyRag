using Microsoft.Extensions.DependencyInjection;
using StudyRag.Configuration;
using StudyRag.Core.Services;

if (args.Length != 0)
{
    Console.WriteLine("Usage: dotnet run --project StudyRag.Evaluation");
    return 1;
}

await using var services = new ServiceCollection()
    .AddStudyRag(new OllamaSettings())
    .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
var indexing = services.GetRequiredService<IndexingService>();
var answering = services.GetRequiredService<QuestionAnsweringService>();
// Explicit files avoid accidentally including files copied from the referenced application.
var directory = Path.Combine(AppContext.BaseDirectory, "Data");
var chunks = (await indexing.IndexAsync(Path.Combine(directory, "precision.txt")))
    .Concat(await indexing.IndexAsync(Path.Combine(directory, "overlap.txt"))).ToArray();

(string Question, string Expected)[] cases =
[
    ("What healthcare provider does Moana want?",
     "A provider incorporating Pasifika health models. Cite a Moana passage; generic cultural advice alone is insufficient."),
    ("What is the name of Moana's preferred healthcare provider?",
     "Say the name is not given in the supplied context. Do not invent an organisation."),
    ("What health condition does Moana have, and what kind of healthcare provider does she want?",
     "Chronic asthma AND a provider incorporating Pasifika health models, supported by a Moana passage."),
    ("Compare Zara's and Tane's barriers to participating in healthcare. What specific support is suggested for each person?",
     "Zara: speech/language barriers, husband answering; address her directly, allow time and communication aids/interpreter. Tane: rural transport barriers to diabetes check-ups; help with transport/appointments. Cite evidence for BOTH people; do not substitute generic advice."),
    ("What exact dose of diabetes medication has Tane been prescribed?",
     "Say the dose is not available in the supplied context. Do not invent a drug or dose."),
    ("Which service lifetime creates a new instance every time the service is requested?",
     "Transient, with a citation to the transient passage. Do not confuse it with scoped or singleton."),
    ("How do scoped and singleton services differ in instance reuse across HTTP requests?",
     "Scoped: one instance within a request scope, different across requests. Singleton: same instance across scopes/requests. Cite BOTH relevant passages."),
    ("According to the supplied material, how many bytes of memory does each scoped service instance use?",
     "Say the byte count is not supplied. Do not invent a number.")
];

Console.WriteLine("Answer evaluation — manual review required, not an automatic accuracy score.");
Console.WriteLine("For each case, check passage relevance, required facts, citation support, and invented details.");
foreach (var (question, expected) in cases)
{
    Console.WriteLine($"\nQUESTION: {question}\nEXPECTED: {expected}");
    var result = await answering.AnswerAsync(question, chunks);
    foreach (var assessment in result.Assessments)
        Console.WriteLine($"Candidate [{assessment.Retrieval.Chunk.SourceFile}, chunk {assessment.Retrieval.Chunk.ChunkNumber}]: selected={assessment.IsSelected}, valid={assessment.IsValid}; {assessment.Reason}");
    foreach (var source in result.Sources)
        Console.WriteLine($"\nSOURCE [{source.Chunk.SourceFile}, chunk {source.Chunk.ChunkNumber}]:\n{source.Chunk.Text}");
    Console.WriteLine($"\nFINAL ANSWER:\n{result.Text}");
    Console.WriteLine("REVIEW: mark pass only if the expected facts are correct, citations support the claims, and no missing detail was invented.");
}
Console.WriteLine("\nCompleted. Review all eight answers above; successful execution does not mean the answers passed.");
return 0;
