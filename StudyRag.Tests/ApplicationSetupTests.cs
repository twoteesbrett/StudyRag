using Microsoft.Extensions.DependencyInjection;
using StudyRag.Configuration;
using StudyRag.ConsoleApp;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class ApplicationSetupTests
{
    [Theory]
    [InlineData("--rerank", AppMode.Chat, true)]
    [InlineData("--evaluate-sample-retrieval", AppMode.SampleRetrieval, false)]
    [InlineData("--evaluate-precision", AppMode.Precision, false)]
    [InlineData("--evaluate-synthetic-overlap", AppMode.SyntheticOverlap, false)]
    [InlineData("--evaluate-reranking", AppMode.Reranking, false)]
    public void Commands_SelectTheExpectedMode(string argument, AppMode mode, bool rerank)
    {
        Assert.True(CommandLineOptions.TryParse([argument], out var options));
        Assert.Equal(mode, options.Mode);
        Assert.Equal(rerank, options.UseReranker);
    }

    [Fact]
    public void DefaultAndInvalidCommands_AreHandledBeforeStartingServices()
    {
        Assert.True(CommandLineOptions.TryParse([], out var options));
        Assert.Equal(new CommandLineOptions(AppMode.Chat), options);
        Assert.False(CommandLineOptions.TryParse(["--unknown"], out _));
        Assert.False(CommandLineOptions.TryParse(["--rerank", "--evaluate-precision"], out _));
        Assert.False(CommandLineOptions.TryParse(["--evaluate-reranking", "--evaluate-precision"], out _));
    }

    [Fact]
    public async Task DependencyInjection_ResolvesBothFlowsAndOwnsHttpClientLifetime()
    {
        var settings = new OllamaSettings
        {
            Endpoint = new Uri("http://localhost:12345"),
            RequestTimeout = TimeSpan.FromMinutes(7)
        };
        var services = new ServiceCollection().AddStudyRag(settings)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        var client = services.GetRequiredService<HttpClient>();
        await using (services)
        {
            Assert.Equal(settings.Endpoint, client.BaseAddress);
            Assert.Equal(settings.RequestTimeout, client.Timeout);
            Assert.NotNull(services.GetRequiredService<ChatSession>());
            Assert.NotNull(services.GetRequiredService<EvaluationRunner>());
            Assert.Same(services.GetRequiredService<EvidenceReranker>(), services.GetRequiredService<EvidenceReranker>());
        }
        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetAsync("/"));
    }
}
