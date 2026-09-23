using System.Text.Json;
using Microsoft.Extensions.AI;
using StudyRag.Core.Helpers;
using StudyRag.Core.Models;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class IndexingServiceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "StudyRagTests-" + Guid.NewGuid());

    public IndexingServiceTests() => Directory.CreateDirectory(directory);

    [Fact]
    public async Task IndexDirectoryAsync_CombinesTextAndMarkdownAndPreservesSources()
    {
        await File.WriteAllTextAsync(Path.Combine(directory, "b.txt"), "Third paragraph.");
        await File.WriteAllTextAsync(Path.Combine(directory, "a.txt"), "First paragraph.\n\nSecond paragraph.");
        await File.WriteAllTextAsync(Path.Combine(directory, "c.MD"), "# Heading\n\nMāori health. ");
        await File.WriteAllTextAsync(Path.Combine(directory, "empty.txt"), "  ");
        await File.WriteAllTextAsync(Path.Combine(directory, "ignored.html"), "Ignore this.");
        var generator = new StubEmbeddingGenerator();
        using var chat = new StubChat();
        var service = new IndexingService(new TextFileLoader(), chat, new EmbeddingService(generator));

        var chunks = await service.IndexDirectoryAsync(directory);

        Assert.Equal(new[] { "a.txt", "a.txt", "b.txt", "c.MD" }, chunks.Select(c => c.SourceFile));
        Assert.Equal(new[] { 1, 2, 1, 1 }, chunks.Select(c => c.ChunkNumber));
        Assert.Equal(new[] { "First paragraph.\n\n", "Second paragraph.", "Third paragraph.", "# Heading\n\nMāori health. " }, chunks.Select(c => c.Text));
        Assert.Equal(3, chat.Calls);
        Assert.Equal(chunks.Select(c => c.Text), generator.Inputs);
        Assert.All(chunks, c => Assert.Equal(new float[] { 1, 0 }, c.Embedding.ToArray()));
    }

    [Fact]
    public async Task IndexDirectoryAsync_EmptyDirectory_ReturnsNoChunks()
    {
        var generator = new StubEmbeddingGenerator();
        using var chat = new StubChat();
        var service = new IndexingService(new TextFileLoader(), chat, new EmbeddingService(generator));

        Assert.Empty(await service.IndexDirectoryAsync(directory));
        Assert.Empty(generator.Inputs);
        Assert.Equal(0, chat.Calls);
    }

    [Fact]
    public async Task IndexAsync_InvalidRanges_DoesNotEmbed()
    {
        var path = Path.Combine(directory, "notes.md");
        await File.WriteAllTextAsync(path, "First.\n\nSecond.");
        using var chat = new StubChat(invalid: true);
        var generator = new StubEmbeddingGenerator();
        var service = new IndexingService(new TextFileLoader(), chat, new EmbeddingService(generator));

        await Assert.ThrowsAsync<InvalidDataException>(() => service.IndexAsync(path));

        Assert.Empty(generator.Inputs);
    }

    [Theory]
    [InlineData("# Title\n\nText.\n\n", "# Title\n\n", "Text.\n\n")]
    [InlineData("First\r\n \t\r\nSecond", "First\r\n \t\r\n", "Second")]
    [InlineData("\n\nFirst\n\n\nSecond", "\n\nFirst\n\n\n", "Second")]
    public async Task IndexAsync_SendsNumberedBlocks(string text, string first, string second)
    {
        using var chat = new StubChat();

        var chunks = await IndexText(text, chat);

        Assert.Equal(new[] { 1, 2 }, chat.Blocks.Select(block => block.Id));
        Assert.Equal(new[] { first, second }, chat.Blocks.Select(block => block.Text));
        Assert.Equal(text, string.Concat(chunks.Select(chunk => chunk.Text)));
    }

    [Theory]
    [InlineData("- First\n- Second\n  - Nested")]
    [InlineData("| Name | Value |\r\n| --- | --- |\r\n| A | B |")]
    [InlineData("Māori health.  \nNext line. ")]
    public async Task IndexAsync_SinglePassage_PreservesText(string text)
    {
        using var chat = new StubChat();

        var chunks = await IndexText(text, chat);

        Assert.Equal(text, Assert.Single(chat.Blocks).Text);
        Assert.Equal(text, Assert.Single(chunks).Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \r\n\t\n")]
    public async Task IndexAsync_EmptyText_SkipsModels(string text)
    {
        using var chat = new StubChat();
        using var generator = new StubEmbeddingGenerator();

        Assert.Empty(await IndexText(text, chat, generator));
        Assert.Equal(0, chat.Calls);
        Assert.Empty(generator.Inputs);
    }

    [Theory]
    [InlineData(". Next sentence ", 3001)]
    [InlineData("\n- Next item ", 3001)]
    [InlineData(" next word ", 3010)]
    public async Task IndexAsync_LongPassage_PrefersBoundaries(string middle, int end)
    {
        var text = new string('a', 3000) + middle + new string('b', 2000);
        using var chat = new StubChat();

        var chunks = await IndexText(text, chat);

        Assert.Equal(text[..end], chat.Blocks[0].Text);
        AssertRequests(text, chat, chunks);
        Assert.All(chat.Blocks, block => Assert.InRange(block.Text.Length, 1, 4000));
    }

    [Theory]
    [InlineData("xy", 4000)]
    [InlineData("\r\n", 3999)]
    [InlineData("😀", 3999)]
    public async Task IndexAsync_HardSplit_PreservesCharacters(string middle, int end)
    {
        var text = new string('a', 3999) + middle + new string('b', 2000);
        using var chat = new StubChat();

        var chunks = await IndexText(text, chat);

        Assert.Equal(text[..end], chat.Blocks[0].Text);
        AssertRequests(text, chat, chunks);
        Assert.All(chat.Blocks, block => Assert.InRange(block.Text.Length, 1, 4000));
    }

    [Fact]
    public async Task IndexAsync_LongPassages_PreservesAllText()
    {
        var text = "\n\n" + new string('a', 5182) + "\r\n\r\n" + new string('b', 4100) + "  ";
        using var chat = new StubChat();

        var chunks = await IndexText(text, chat);

        AssertRequests(text, chat, chunks);
        Assert.True(chat.Calls > 1);
        Assert.All(chat.Blocks, block => Assert.InRange(block.Text.Length, 1, 4000));
    }

    [Fact]
    public async Task IndexAsync_ModelBreaks_ControlGrouping()
    {
        using var chat = new StubChat(response: """{"breaks":[2]}""");
        using var generator = new StubEmbeddingGenerator();

        var chunks = await IndexText("# Title\r\n\r\nMāori health.\n\nLast. ", chat, generator);

        Assert.Equal(new[] { "# Title\r\n\r\nMāori health.\n\n", "Last. " }, chunks.Select(chunk => chunk.Text));
        Assert.Equal(chunks.Select(chunk => chunk.Text), generator.Inputs);
    }

    [Theory]
    [InlineData("{\"id\":105,\"text\":\"Great job completing the module\",\"length\":161}")]
    [InlineData("not JSON")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"breaks\":null}")]
    [InlineData("{\"breaks\":[0]}")]
    [InlineData("{\"breaks\":[1.5]}")]
    [InlineData("{\"breaks\":[1,1]}")]
    [InlineData("{\"breaks\":[2]}")]
    [InlineData("{\"breaks\":[34]}")]
    public async Task IndexAsync_InvalidResponse_DoesNotEmbed(string response)
    {
        using var chat = new StubChat(response: response);
        using var generator = new StubEmbeddingGenerator();

        await Assert.ThrowsAsync<InvalidDataException>(() => IndexText("A\n\nB", chat, generator));

        Assert.Empty(generator.Inputs);
    }

    [Fact]
    public async Task IndexAsync_RangeOutsideRequest_DoesNotEmbed()
    {
        using var chat = new StubChat(response: """{"breaks":[2]}""");
        using var generator = new StubEmbeddingGenerator();
        var text = new string('a', 2500) + "\n\n" + new string('b', 2500);

        await Assert.ThrowsAsync<InvalidDataException>(() => IndexText(text, chat, generator));

        Assert.Empty(generator.Inputs);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(31)]
    public async Task IndexAsync_RequiresBoundarySchema(int count)
    {
        using var chat = new StubChat();

        await IndexText(string.Join("\n\n", Enumerable.Range(1, count).Select(i => $"Block {i}")), chat);

        var format = Assert.IsType<ChatResponseFormatJson>(chat.Format);
        var schema = Assert.IsType<JsonElement>(format.Schema);
        Assert.Equal("breaks", schema.GetProperty("required")[0].GetString());
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        var breaks = schema.GetProperty("properties").GetProperty("breaks");
        Assert.Equal(count - 1, breaks.GetProperty("maxItems").GetInt32());
        Assert.True(breaks.GetProperty("uniqueItems").GetBoolean());
        var item = breaks.GetProperty("items");
        Assert.Equal(1, item.GetProperty("minimum").GetInt32());
        Assert.Equal(count - 1, item.GetProperty("maximum").GetInt32());
    }

    [Fact]
    public async Task IndexAsync_InventedBlockIds_DoesNotEmbed()
    {
        using var chat = new StubChat(response: """{"chunks":[{"start":1,"end":5},{"start":6,"end":10},{"start":11,"end":15},{"start":16,"end":21},{"start":22,"end":28},{"start":29,"end":30},{"start":31,"end":34}]}""");
        using var generator = new StubEmbeddingGenerator();
        var text = string.Join("\n\n", Enumerable.Range(1, 31).Select(i => $"Block {i}"));

        await Assert.ThrowsAsync<InvalidDataException>(() => IndexText(text, chat, generator));

        Assert.Empty(generator.Inputs);
    }

    [Theory]
    [InlineData("{\"breaks\":[4,10,19,28]}", 5)]
    [InlineData("{\"breaks\":[]}", 1)]
    public async Task IndexAsync_Breaks_CoverAllBlocksOnce(string response, int count)
    {
        using var chat = new StubChat(response: response);
        var text = string.Join("\n\n", Enumerable.Range(1, 31).Select(i => $"Block {i}"));

        var chunks = await IndexText(text, chat);

        Assert.Equal(count, chunks.Count);
        Assert.Equal(text, string.Concat(chunks.Select(chunk => chunk.Text)));
        Assert.EndsWith("Block 31", chunks[^1].Text);
    }

    [Theory]
    [InlineData("{\"breaks\":[4,4]}")]
    [InlineData("{\"breaks\":[10,4]}")]
    [InlineData("{\"breaks\":[4,10,19,28,31,31]}")]
    public async Task IndexAsync_InvalidBreakOrder_DoesNotEmbed(string response)
    {
        using var chat = new StubChat(response: response);
        using var generator = new StubEmbeddingGenerator();
        var text = string.Join("\n\n", Enumerable.Range(1, 31).Select(i => $"Block {i}"));

        await Assert.ThrowsAsync<InvalidDataException>(() => IndexText(text, chat, generator));

        Assert.Empty(generator.Inputs);
    }

    [Fact]
    public async Task IndexAsync_LongMarkdown_SendsSmallOverlappingRequests()
    {
        var text = string.Concat(Enumerable.Range(1, 100).Select(i => $"Paragraph {i}: " + new string('x', 90) + "\r\n\r\n"));
        using var chat = new StubChat();

        var chunks = await IndexText(text, chat);

        Assert.True(chat.Calls > 1);
        AssertRequests(text, chat, chunks);
        Assert.Equal(Enumerable.Range(1, chunks.Count), chunks.Select(chunk => chunk.ChunkNumber));
    }

    private static void AssertRequests(string source, StubChat chat, IReadOnlyList<DocumentChunk> chunks)
    {
        var end = 0;

        foreach (var request in chat.Requests)
        {
            var part = string.Concat(request.Select(block => block.Text));
            Assert.InRange(part.Length, 1, 4000);
            Assert.Equal(Enumerable.Range(1, request.Length), request.Select(block => block.Id));
            var start = end == 0 ? 0 : end - 200;

            if (start > 0 && ((source[start - 1] == '\r' && source[start] == '\n') ||
                (char.IsHighSurrogate(source[start - 1]) && char.IsLowSurrogate(source[start]))))
            {
                start++;
            }

            Assert.Equal(source.Substring(start, part.Length), part);
            Assert.True(start + part.Length > end);
            end = start + part.Length;
        }

        Assert.Equal(source.Length, end);
        Assert.Equal(string.Concat(chat.Blocks.Select(block => block.Text)), string.Concat(chunks.Select(chunk => chunk.Text)));
    }

    private async Task<IReadOnlyList<DocumentChunk>> IndexText(
        string text, StubChat chat, StubEmbeddingGenerator? generator = null)
    {
        var path = Path.Combine(directory, "notes.md");
        await File.WriteAllTextAsync(path, text);
        var service = new IndexingService(new TextFileLoader(), chat,
            new EmbeddingService(generator ?? new StubEmbeddingGenerator()));

        return await service.IndexAsync(path);
    }

    private sealed class StubChat(bool invalid = false, string? response = null) : IChatClient
    {
        public int Calls { get; private set; }
        public ChatResponseFormat? Format { get; private set; }
        public List<TextBlock> Blocks { get; } = [];
        public List<TextBlock[]> Requests { get; } = [];

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            Format = options?.ResponseFormat;
            using var json = JsonDocument.Parse(messages.Last().Text);
            var blocks = json.RootElement;
            var request = blocks.EnumerateArray().Select(block => new TextBlock(
                block.GetProperty("id").GetInt32(), block.GetProperty("text").GetString()!)).ToArray();
            Requests.Add(request);
            Blocks.AddRange(request);
            var heading = blocks[0].GetProperty("text").GetString()!.StartsWith('#');
            var breaks = heading ? Array.Empty<int>() : Enumerable.Range(1, blocks.GetArrayLength() - 1).ToArray();
            var reply = response ?? (invalid ? "{\"breaks\":[0]}" : JsonSerializer.Serialize(new { breaks }));

            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    public void Dispose()
    {
        foreach (var path in Directory.EnumerateFiles(directory)) File.Delete(path);
        Directory.Delete(directory);
    }

    private sealed class StubEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public List<string> Inputs { get; } = [];

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values, EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var result = new GeneratedEmbeddings<Embedding<float>>();
            foreach (var value in values)
            {
                Inputs.Add(value);
                result.Add(new Embedding<float>(new float[] { 1, 0 }));
            }
            return Task.FromResult(result);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
