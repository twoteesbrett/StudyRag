using Microsoft.Extensions.DependencyInjection;
using StudyRag.Configuration;
using StudyRag.ConsoleApp;

if (args.Length > 0)
{
    Console.WriteLine("Usage: dotnet run --project StudyRag");
    Console.WriteLine("Ask questions about the text files in Data. Type 'exit' to quit.");

    return args.SequenceEqual(["--help"]) ? 0 : 1;
}

await using var services = new ServiceCollection()
    .AddStudyRag(new OllamaSettings())
    .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

await services
    .GetRequiredService<Chat>()
    .RunAsync(Path.Combine(AppContext.BaseDirectory, "Data"));

return 0;
