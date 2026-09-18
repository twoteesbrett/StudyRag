using Microsoft.Extensions.DependencyInjection;
using StudyRag.Configuration;
using StudyRag.ConsoleApp;
using StudyRag.Core.Services;

namespace StudyRag.Tests;

public class ApplicationSetupTests
{
    [Fact]
    public async Task DependencyInjection_ResolvesAnsweringFlowAndOwnsHttpClientLifetime()
    {
        var settings = new OllamaSettings
        {
            Endpoint = new Uri("http://localhost:12345"),
            RequestTimeout = TimeSpan.FromMinutes(7)
        };
        var services = new ServiceCollection().AddStudyRag(settings)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        var client = services.GetRequiredService<HttpClient>();
        await using (services)
        {
            Assert.Equal(settings.Endpoint, client.BaseAddress);
            Assert.Equal(settings.RequestTimeout, client.Timeout);
            Assert.NotNull(services.GetRequiredService<Chat>());
            Assert.NotNull(services.GetRequiredService<QuestionAnsweringService>());
            Assert.Same(services.GetRequiredService<EvidenceAssessmentService>(), services.GetRequiredService<EvidenceAssessmentService>());
        }
        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetAsync("/"));
    }
}
