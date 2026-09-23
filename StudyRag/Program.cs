using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;

using StudyRag.Core.Helpers;
using StudyRag.Core.Services;

if (args.Length > 0)
{
    Console.WriteLine("Usage: dotnet run --project StudyRag");
    Console.WriteLine("Ask questions about the text and Markdown files in Data. Type 'exit' to quit.");

    return args.SequenceEqual(["--help"]) ? 0 : 1;
}

var settings = new Settings();
var registrations = new ServiceCollection();

registrations.AddSingleton(settings);

registrations.AddSingleton(_ => new HttpClient
{
    BaseAddress = settings.Endpoint,
    Timeout = settings.RequestTimeout
});

registrations.AddSingleton<IChatClient>(provider => new OllamaApiClient(
    provider.GetRequiredService<HttpClient>(), settings.ChatModel));

registrations.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(provider => new OllamaApiClient(
    provider.GetRequiredService<HttpClient>(), settings.EmbeddingModel));

registrations.AddSingleton<Action<ChatOptions>>(_ => options =>
    options.RawRepresentationFactory = _ => new ChatRequest { Think = false });

registrations.AddLogging(logging => logging
    .SetMinimumLevel(LogLevel.Trace)
    .AddFilter("Microsoft", LogLevel.Warning)
    .AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    }));

registrations.AddSingleton<TextFileLoader>();
registrations.AddSingleton<EmbeddingService>();
registrations.AddSingleton<IndexingService>();
registrations.AddSingleton<RetrievalService>();
registrations.AddSingleton<EvidenceAssessmentService>();
registrations.AddSingleton<RagService>();
registrations.AddSingleton<QuestionAnsweringService>();
registrations.AddTransient<Chat>();

await using var services = registrations.BuildServiceProvider(
    new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

var dataPath = Path.Combine(AppContext.BaseDirectory, "Data");

var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("StudyRag");

try
{
    var indexing = services.GetRequiredService<IndexingService>();
    var chunks = await indexing.IndexDirectoryAsync(dataPath);

    var chat = services.GetRequiredService<Chat>();
    await chat.RunAsync(chunks);

    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "StudyRag stopped before completing its work.");
    return 1;
}
