using Microsoft.Extensions.AI;
using StudyRag.Core.Models;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class QuestionAnsweringServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AnswerAsync_OnlyPassesSupportedEvidenceToGeneration(bool malformed)
    {
        using var embeddings = new StubEmbeddings();
        using var client = new StubChat(malformed);
        var service = new QuestionAnsweringService(
            new RetrievalService(new EmbeddingService(embeddings)),
            new EvidenceAssessmentService(client), new RagService(client));
        DocumentChunk[] chunks =
        [
            new() { SourceFile = "notes.txt", ChunkNumber = 1, Text = "Generic topical advice.", Embedding = new float[] { 1, 0 } },
            new() { SourceFile = "notes.txt", ChunkNumber = 2, Text = "A has the first fact.", Embedding = new float[] { 1, 0 } },
            new() { SourceFile = "notes.txt", ChunkNumber = 3, Text = "B has the second fact.", Embedding = new float[] { 1, 0 } }
        ];

        var answer = await service.AnswerAsync("Compare A and B", chunks);

        Assert.Equal(new[] { 2, 3 }, answer.Sources.Select(s => s.Chunk.ChunkNumber));
        Assert.Equal(!malformed, answer.Assessments.Single(a => a.Retrieval.Chunk.ChunkNumber == 1).IsValid);
        Assert.DoesNotContain("Generic topical advice.", client.GenerationPrompt);
        Assert.Contains("[notes.txt, chunk 2]", client.GenerationPrompt);
        Assert.Contains("A has the first fact.", client.GenerationPrompt);
        Assert.Contains("[notes.txt, chunk 3]", client.GenerationPrompt);
        Assert.Contains("B has the second fact.", client.GenerationPrompt);
        Assert.Equal("Generated answer", answer.Text);
    }

    [Fact]
    public async Task NoSelectedEvidence_PassesCandidatesAsClearlyLabelledRelatedContext()
    {
        using var embeddings = new StubEmbeddings();
        using var client = new StubChat(false, rejectAll: true);
        var service = new QuestionAnsweringService(
            new RetrievalService(new EmbeddingService(embeddings)),
            new EvidenceAssessmentService(client), new RagService(client));
        DocumentChunk[] chunks =
        [
            new()
            {
                SourceFile = "precision.txt",
                ChunkNumber = 21,
                Text = "Moana has chronic asthma.",
                Embedding = new float[] { 1, 0 }
            }
        ];

        var answer = await service.AnswerAsync("Why does Moana need help managing diabetes?", chunks);

        Assert.Empty(answer.Sources);
        Assert.Single(answer.RelatedSources);
        Assert.Contains("using only the retrieved passages", client.GenerationPrompt);
        Assert.Contains("Retrieved passages:", client.GenerationPrompt);
        Assert.Contains("[precision.txt, chunk 21]", client.GenerationPrompt);
        Assert.Contains("Moana has chronic asthma.", client.GenerationPrompt);
        Assert.Contains("question contains a false premise", client.GenerationPrompt);
        Assert.Contains("If it is attributed to another named person", client.GenerationPrompt);
        Assert.Contains("inline citation immediately after every factual statement", client.GenerationPrompt);
        Assert.Contains("used only to correct the question", client.GenerationPrompt);
        Assert.Contains("Do not produce a separate citation list", client.GenerationPrompt);
        Assert.Contains("Do not end with an offer to help", client.GenerationPrompt);
        Assert.DoesNotContain("Answer using your general knowledge", client.GenerationPrompt);
    }
    [Fact]
    public async Task EmptyCorpus_SkipsRetrievalAndEvidenceCallsAndMarksMissingContext()
    {
        using var embeddings = new StubEmbeddings();
        using var client = new StubChat(false);
        var service = new QuestionAnsweringService(
            new RetrievalService(new EmbeddingService(embeddings)),
            new EvidenceAssessmentService(client), new RagService(client));

        var answer = await service.AnswerAsync("Question", []);

        Assert.Empty(answer.Sources);
        Assert.Empty(answer.Assessments);
        Assert.Equal(0, embeddings.Calls);
        Assert.Equal(0, client.EvidenceCalls);
        Assert.Contains("No document context was supplied.", client.GenerationPrompt);
        Assert.Contains("Never substitute a similarly named real or fictional person", client.GenerationPrompt);
        Assert.Contains("no context means you must only say", client.GenerationPrompt);
        Assert.Contains("do not add generic background", client.GenerationPrompt);
    }

    private sealed class StubEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
    {
        public int Calls { get; private set; }
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            var results = new GeneratedEmbeddings<Embedding<float>>();
            foreach (var value in values)
                results.Add(new Embedding<float>(new float[] { 1, 0 }));
            return Task.FromResult(results);
        }
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private sealed class StubChat(bool malformed, bool rejectAll = false) : IChatClient
    {
        public string GenerationPrompt { get; private set; } = "";
        public int EvidenceCalls { get; private set; }
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var text = string.Join("\n", messages.Select(m => m.Text));
            string response;
            if (options?.ResponseFormat == ChatResponseFormat.Json)
            {
                EvidenceCalls++;
                response = rejectAll
                    ? """{"score":0,"evidence":0,"reason":"No direct answer"}"""
                    : text.Contains("Generic topical advice.")
                        ? malformed ? "invalid JSON" : """{"score":1,"evidence":0,"reason":"Topical only"}"""
                        : """{"score":2,"evidence":1,"reason":"One side of comparison"}""";
            }
            else
            {
                GenerationPrompt = text;
                response = "Generated answer";
            }
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, response)));
        }
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
