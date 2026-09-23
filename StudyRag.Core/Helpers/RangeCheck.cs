using StudyRag.Core.Models;

namespace StudyRag.Core.Helpers;

public static class RangeCheck
{
    /// <summary>Checks that ranges cover block IDs 1 through count exactly once, in order.</summary>
    public static bool IsValid(IReadOnlyList<ChunkRange> ranges, int count)
    {
        ArgumentNullException.ThrowIfNull(ranges);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var end = 0;

        foreach (var range in ranges)
        {
            if (range is null || end == count || range.Start != end + 1 ||
                range.End < range.Start || range.End > count)
            {
                return false;
            }

            end = range.End;
        }

        return end == count;
    }
}
