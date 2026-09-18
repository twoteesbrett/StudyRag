using Microsoft.Extensions.DependencyInjection;
using StudyRag.Configuration;
using StudyRag.ConsoleApp;

if (!CommandLineOptions.TryParse(args, out var options))
{
    Console.WriteLine(CommandLineOptions.Help);
    return args.SequenceEqual(["--help"]) ? 0 : 1;
}

await using var services = new ServiceCollection()
    .AddStudyRag(new OllamaSettings())
    .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

if (options.Mode == AppMode.Chat)
{
    await services.GetRequiredService<ChatSession>().RunAsync(dataDirectory, options.UseReranker);
}
else
{
    await services.GetRequiredService<EvaluationRunner>().RunAsync(options.Mode, dataDirectory);
}

return 0;
