public sealed class Settings
{
    public Uri Endpoint { get; init; } = new("http://192.168.1.11:11434");
    //public Uri Endpoint { get; init; } = new("http://localhost:11434");
    public string ChatModel { get; init; } = "qwen2.5";
    public string EmbeddingModel { get; init; } = "nomic-embed-text";
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromMinutes(10);
}
