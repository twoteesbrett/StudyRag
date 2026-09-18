using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using StudyRag.ConsoleApp;
using StudyRag.Core.Services;
using StudyRag.Helpers;

namespace StudyRag.Configuration;

public static class ServiceRegistration
{
    public static IServiceCollection AddStudyRag(this IServiceCollection services, OllamaSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<OllamaSettings>();
            return new HttpClient { BaseAddress = options.Endpoint, Timeout = options.RequestTimeout };
        });
        services.AddSingleton<IChatClient>(provider => new OllamaApiClient(
            provider.GetRequiredService<HttpClient>(), settings.ChatModel));
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(provider => new OllamaApiClient(
            provider.GetRequiredService<HttpClient>(), settings.EmbeddingModel));
#if DEBUG
        const LogLevel minimumLevel = LogLevel.Debug;
#else
        const LogLevel minimumLevel = LogLevel.Information;
#endif
        services.AddLogging(logging => logging.SetMinimumLevel(minimumLevel)
            .AddProvider(new DebugConsoleLoggerProvider()));
        services.AddSingleton<TextFileLoader>();
        services.AddSingleton<EmbeddingService>();
        services.AddSingleton<IndexingService>();
        services.AddSingleton<RetrievalService>();
        services.AddSingleton<EvidenceReranker>();
        services.AddSingleton<RagService>();
        services.AddTransient<ChatSession>();
        services.AddTransient<EvaluationRunner>();
        return services;
    }
}
