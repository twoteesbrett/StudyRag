using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class VectorMathTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        float[] a = [1, 2, 3];

        var result = VectorMath.CosineSimilarity(a, a);

        Assert.Equal(1f, result, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        float[] a = [1, 0];
        float[] b = [0, 1];

        var result = VectorMath.CosineSimilarity(a, b);

        Assert.Equal(0f, result, precision: 5);
    }
}