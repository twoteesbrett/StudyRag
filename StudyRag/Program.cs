using Microsoft.Extensions.AI;
using OllamaSharp;
using StudyRag.Helpers;
using StudyRag.Models;
using StudyRag.Services;

var ollamaUri = new Uri("http://192.168.1.11:11434");
//var ollamaUri = new Uri("http://localhost:11434");

IChatClient chatClient =
    new OllamaApiClient(
        ollamaUri,
        "qwen2.5");

IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    new OllamaApiClient(
        ollamaUri,
        "nomic-embed-text");

var embeddingService =
    new EmbeddingService(embeddingGenerator);

var retrievalService =
    new RetrievalService(embeddingService);

var ragService =
    new RagService(chatClient);

var indexingService = new IndexingService(
    new TextFileLoader(),
    embeddingService);

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

if (args.Contains("--evaluate"))
{
    var evaluationChunks = await indexingService.IndexAsync(
        Path.Combine(dataDirectory, "sample1.txt"));
    await RetrievalEvaluation.RunAsync(retrievalService, evaluationChunks);
    return;
}

var chunks = await indexingService.IndexDirectoryAsync(dataDirectory);

if (chunks.Count == 0)
{
    Console.WriteLine("No text was found. Add .txt files to the Data folder and run again.");
    return;
}

while (true)
{
    Console.Write("\nAsk a question (or type 'exit'): ");
    var question = Console.ReadLine();

    if (question is null ||
        question.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(question))
    {
        continue;
    }

    var matches = await retrievalService.FindBestMatchesAsync(
        question,
        chunks,
        count: 3,
        minSimilarity: 0.60f);

#if DEBUG
    DebugConsole.WriteHeader("RETRIEVED CHUNKS");
    DebugConsole.WriteLine($"Found {matches.Count} matching chunks.");

    foreach (var match in matches)
    {
        DebugConsole.WriteLine(
            $"Source: {match.Chunk.SourceFile}, chunk {match.Chunk.ChunkNumber}");
        DebugConsole.WriteLine($"Similarity: {match.Similarity:F3}");
        DebugConsole.WriteLine(match.Chunk.Text);
        DebugConsole.WriteLine("");
    }
#endif

    if (matches.Count == 0)
    {
        Console.WriteLine(
            "No sufficiently relevant information was found.");
        continue;
    }

    var answer = await ragService.AskAsync(question, matches);

    Console.WriteLine($"\nAnswer:\n{answer}");
}
