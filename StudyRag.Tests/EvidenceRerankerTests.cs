using Microsoft.Extensions.AI;
using StudyRag.Core.Models;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class EvidenceRerankerTests
{
    private static RetrievalResult Candidate(string text, int number = 1, float similarity = 0.8f) =>
        new(new DocumentChunk { SourceFile = "test.txt", ChunkNumber = number, Text = text, Embedding = ReadOnlyMemory<float>.Empty }, similarity);

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{}")]
    [InlineData("{\"score\":4,\"evidence\":1,\"reason\":\"Present\"}")]
    [InlineData("{\"score\":3,\"evidence\":99,\"reason\":\"Present\"}")]
    [InlineData("{\"score\":2,\"evidence\":0,\"reason\":\"Present\"}")]
    public void InvalidAssessment_CannotSelectPassage(string response)
    {
        var result = EvidenceAssessmentService.Parse(Candidate("Fact"), response);
        Assert.False(result.IsSelected);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RejectionWithExistingSentenceReference_RemainsValidAndUnselected()
    {
        var result = EvidenceAssessmentService.Parse(Candidate("A provider with preferred characteristics."),
            """{"evidence":1,"reason":"No provider name is stated","score":0}""");
        Assert.True(result.IsValid);
        Assert.False(result.IsSelected);
        Assert.Empty(result.Evidence);
    }

    [Fact]
    public void EvidenceId_ResolvesToVerbatimSourceSentence()
    {
        var candidate = Candidate("First fact. A second fact with Māori text!");
        var result = EvidenceAssessmentService.Parse(candidate,
            """{"evidence":2,"reason":"Second sentence answers it","score":3}""");
        Assert.True(result.IsSelected);
        Assert.Equal("A second fact with Māori text!", result.Evidence);
    }

    [Fact]
    public async Task RankAsync_KeepsBothComparisonSidesAndRejectsTopicalMatch()
    {
        using var client = new StubChatClient();
        var results = await new EvidenceAssessmentService(client).AssessAsync("Compare A and B",
            [Candidate("Generic advice", 1, 0.9f), Candidate("A fact", 2, 0.7f), Candidate("B fact", 3, 0.6f)]);
        Assert.Equal(new[] { 2, 3 }, results.Where(r => r.IsSelected).Select(r => r.Retrieval.Chunk.ChunkNumber));
        Assert.Equal(3, client.Calls);
        Assert.Empty(await new EvidenceAssessmentService(client).AssessAsync("Empty", []));
        Assert.Equal(3, client.Calls);
    }


    [Fact]
    public async Task RankAsync_InstructsModelToKeepCorrectiveAndMissingDetailContext()
    {
        using var client = new StubChatClient();

        await new EvidenceAssessmentService(client).AssessAsync(
            "Why does Moana need help managing her diabetes?",
            [Candidate("Moana has chronic asthma.")]);

        Assert.Contains("corrects a false premise", client.LastPrompt);
        Assert.Contains("question asks for a missing name, dose, address", client.LastPrompt);
        Assert.Contains("Moana has asthma", client.LastPrompt);
        Assert.Contains("Do not use score 2 for generic topical material", client.LastPrompt);
    }
    private sealed class StubChatClient : IChatClient
    {
        public int Calls { get; private set; }
        public string LastPrompt { get; private set; } = "";
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastPrompt = string.Join(Environment.NewLine, messages.Select(message => message.Text));
            var response = Calls switch
            {
                1 => "{\"score\":1,\"evidence\":0,\"reason\":\"Generic only\"}",
                2 => "{\"score\":2,\"evidence\":1,\"reason\":\"First side\"}",
                _ => "{\"score\":2,\"evidence\":1,\"reason\":\"Second side\"}"
            };
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, response)));
        }
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
