using Microsoft.Extensions.Logging;

public sealed class Settings
{
    public Uri Endpoint { get; init; } = new("http://localhost:11434");
    public string ChatModel { get; init; } = "qwen3.5";
    public string EmbeddingModel { get; init; } = "nomic-embed-text";
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromMinutes(10);
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Debug;
}
