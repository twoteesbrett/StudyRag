using System.Security.Cryptography;

namespace StudyRag.Preprocessor;

/// <summary>Saves embedded images without resizing or changing their contents.</summary>
public static class EmbeddedImageSaver
{
    public static string Save(string dataUri, string outputDirectory)
    {
        var comma = dataUri.IndexOf(',');
        if (comma < 0)
            throw new FormatException("The image data URI has no payload separator.");

        var parts = dataUri[5..comma].Split(';');
        var extension = parts[0].ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            "image/avif" => ".avif",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            "image/x-icon" or "image/vnd.microsoft.icon" => ".ico",
            _ => throw new FormatException($"Unsupported embedded image type: {parts[0]}")
        };

        if (!parts[^1].Equals("base64", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Only base64 image data is currently saved to files.");

        var bytes = Convert.FromBase64String(Uri.UnescapeDataString(dataUri[(comma + 1)..]));
        if (bytes.Length == 0)
            throw new FormatException("The embedded image is empty.");

        // Content-based names let repeated images share a file within the output folder.
        var fileName = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() + extension;
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllBytes(Path.Combine(outputDirectory, fileName), bytes);

        return fileName;
    }
}
