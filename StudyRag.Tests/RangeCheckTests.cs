using StudyRag.Core.Helpers;
using StudyRag.Core.Models;

namespace StudyRag.Tests;

public class RangeCheckTests
{
    [Theory]
    [InlineData(0, new int[] { })]
    [InlineData(1, new[] { 1, 1 })]
    [InlineData(5, new[] { 1, 5 })]
    [InlineData(5, new[] { 1, 2, 3, 5 })]
    public void IsValid_CompleteCoverage_ReturnsTrue(int count, int[] bounds)
    {
        var ranges = bounds.Chunk(2).Select(pair => new ChunkRange(pair[0], pair[1])).ToArray();

        Assert.True(RangeCheck.IsValid(ranges, count));
    }

    [Theory]
    [InlineData(3, new int[] { })]
    [InlineData(3, new[] { 2, 3 })]             // Missing first block.
    [InlineData(3, new[] { 1, 2 })]             // Missing last block.
    [InlineData(3, new[] { 1, 1, 3, 3 })]       // Gap.
    [InlineData(3, new[] { 1, 2, 2, 3 })]       // Overlap.
    [InlineData(3, new[] { 3, 3, 1, 2 })]       // Reordered.
    [InlineData(3, new[] { 1, 3, 1, 3 })]       // Duplicate.
    [InlineData(3, new[] { 1, 1, 2, 1, 2, 3 })] // Reversed range.
    [InlineData(3, new[] { 0, 3 })]             // Invalid start.
    [InlineData(3, new[] { 1, 4 })]             // Invalid end.
    [InlineData(0, new[] { 1, 1 })]             // No source blocks.
    public void IsValid_InvalidCoverage_ReturnsFalse(int count, int[] bounds)
    {
        var ranges = bounds.Chunk(2).Select(pair => new ChunkRange(pair[0], pair[1])).ToArray();

        Assert.False(RangeCheck.IsValid(ranges, count));
    }

    [Fact]
    public void IsValid_NullRange_ReturnsFalse()
    {
        Assert.False(RangeCheck.IsValid([null!], 1));
    }

    [Fact]
    public void IsValid_NullList_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RangeCheck.IsValid(null!, 1));
    }

    [Fact]
    public void IsValid_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RangeCheck.IsValid([], -1));
    }
}
