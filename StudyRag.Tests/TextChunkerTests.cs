using StudyRag.Services;

namespace StudyRag.Tests;

public class TextChunkerTests
{
    [Fact]
    public void Chunk_ShortParagraphs_PreservesParagraphs()
    {
        var chunks = TextChunker.Chunk(" First paragraph.\r\n\r\nSecond paragraph.\n\n ");

        Assert.Equal(new[] { "First paragraph.", "Second paragraph." }, chunks);
    }

    [Fact]
    public void Chunk_LongParagraph_PrefersSentenceBoundary()
    {
        var chunks = TextChunker.Chunk("First sentence. Second sentence is longer.", 25, 0);

        Assert.Equal("First sentence.", chunks[0]);
        Assert.Equal("First sentence. Second sentence is longer.", string.Join(" ", chunks));
        Assert.All(chunks, chunk => Assert.InRange(chunk.Length, 1, 25));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(9)]
    public void Chunk_UnbrokenText_PreservesContentAndOverlap(int overlap)
    {
        const string text = "abcdefghijklmnopqrstuvwxyz0123456789";
        var chunks = TextChunker.Chunk(text, 10, overlap);
        var reconstructed = chunks[0];

        for (var i = 1; i < chunks.Count; i++)
        {
            Assert.Equal(chunks[i - 1][^overlap..], chunks[i][..overlap]);
            reconstructed += chunks[i][overlap..];
        }

        Assert.Equal(text, reconstructed);
        Assert.All(chunks, chunk => Assert.InRange(chunk.Length, 1, 10));
    }

    [Fact]
    public void Chunk_Whitespace_ReturnsNoChunks()
    {
        Assert.Empty(TextChunker.Chunk(" \r\n\r\n \n\n"));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, -1)]
    [InlineData(10, 10)]
    public void Chunk_InvalidLimits_Throws(int maximum, int overlap)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TextChunker.Chunk("text", maximum, overlap));
    }
}
