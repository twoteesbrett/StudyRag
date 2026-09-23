using Microsoft.Extensions.AI;
using OllamaSharp;
using StudyRag.Core.Helpers;
using StudyRag.Core.Models;
using StudyRag.Core.Services;

namespace StudyRag.IntegrationTests;

public sealed class OllamaTheoryAttribute : TheoryAttribute
{
    public OllamaTheoryAttribute()
    {
        if (Environment.GetEnvironmentVariable("STUDYRAG_INTEGRATION_TESTS") != "1")
            Skip = "Set STUDYRAG_INTEGRATION_TESTS=1 to run against live Ollama models.";
    }
}

// One collection indexes once and serializes cases to avoid competing for model memory.
[CollectionDefinition("Ollama", DisableParallelization = true)]
public sealed class OllamaCollection : ICollectionFixture<OllamaFixture> { }

public sealed class OllamaFixture : IAsyncLifetime
{
    private IChatClient? _chat;
    private IEmbeddingGenerator<string, Embedding<float>>? _embeddings;
    private HttpClient? _httpClient;

    public IChatClient Judge { get; private set; } = null!;
    public QuestionAnsweringService AnsweringService { get; private set; } = null!;
    public IReadOnlyList<DocumentChunk> Chunks { get; private set; } = [];
    public string Configuration { get; private set; } = "";

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("STUDYRAG_INTEGRATION_TESTS") != "1") return;

        var defaults = new Settings();

        var settings = new Settings
        {
            Endpoint = new Uri(Environment.GetEnvironmentVariable("STUDYRAG_OLLAMA_ENDPOINT") ?? defaults.Endpoint.ToString()),
            ChatModel = Environment.GetEnvironmentVariable("STUDYRAG_CHAT_MODEL") ?? defaults.ChatModel,
            EmbeddingModel = Environment.GetEnvironmentVariable("STUDYRAG_EMBEDDING_MODEL") ?? defaults.EmbeddingModel
        };

        var judgeModel = Environment.GetEnvironmentVariable("STUDYRAG_JUDGE_MODEL") ?? settings.ChatModel;

        Configuration = $"Endpoint={settings.Endpoint}; answer={settings.ChatModel}; embeddings={settings.EmbeddingModel}; judge={judgeModel}";

        _httpClient = new HttpClient { BaseAddress = settings.Endpoint, Timeout = settings.RequestTimeout };

        Judge = new OllamaApiClient(_httpClient, judgeModel);
        _chat = new OllamaApiClient(_httpClient, settings.ChatModel);
        _embeddings = new OllamaApiClient(_httpClient, settings.EmbeddingModel);
        var embeddings = new EmbeddingService(_embeddings);

        AnsweringService = new QuestionAnsweringService(
            new RetrievalService(embeddings), new EvidenceAssessmentService(_chat), new RagService(_chat));

        var indexing = new IndexingService(new TextFileLoader(), _chat, embeddings);
        var chunks = new List<DocumentChunk>();

        // Explicit production documents, including unrelated material as retrieval distractors.
        foreach (var file in new[] { "precision.txt", "sample1.txt", "sample2.txt", "sample3.txt" })
        {
            chunks.AddRange(await indexing.IndexAsync(Path.Combine(AppContext.BaseDirectory, "Corpus", file)));
        }

        Chunks = chunks;
    }

    public Task DisposeAsync()
    {
        Judge?.Dispose();
        _chat?.Dispose();
        _embeddings?.Dispose();
        _httpClient?.Dispose();
        return Task.CompletedTask;
    }
}
