namespace StudyRag.Services;

public class TextFileLoader
{
    public async Task<string> LoadAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }
}