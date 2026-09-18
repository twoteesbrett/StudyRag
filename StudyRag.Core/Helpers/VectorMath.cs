namespace StudyRag.Core.Helpers;

public static class VectorMath
{
    public static float CosineSimilarity(
        ReadOnlySpan<float> a,
        ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                "Vectors must have the same dimensions.");

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (var i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        return dotProduct /
            (MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB));
    }
}
