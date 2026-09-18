using Xunit.Abstractions;

namespace StudyRag.IntegrationTests;

[Collection("Ollama")]
[Trait("Category", "Integration")]
public sealed class AnswerQualityTests(OllamaFixture fixture, ITestOutputHelper output)
{
    [OllamaTheory]
    [InlineData("Direct fact")]
    [InlineData("Precision")]
    [InlineData("Missing detail")]
    [InlineData("Overlapping topics")]
    [InlineData("Multiple passages")]
    [InlineData("Partially answerable")]
    [InlineData("Incorrect premise")]
    [InlineData("Similar concepts")]
    [InlineData("Unsupported inference")]
    [InlineData("Cross-topic retrieval")]
    [InlineData("Basic sanity check")]
    public async Task Answer_meets_expectations(string caseName)
    {
        var testCase = AnswerCases.All[caseName];
        var evaluator = new AnswerEvaluator(fixture.Judge, output);

        output.WriteLine(fixture.Configuration);
        output.WriteLine($"Case: {caseName}");
        output.WriteLine($"Question: {testCase.Question}");
        output.WriteLine($"Expected: {testCase.ExpectedAnswer}");

        var answer = await fixture.AnsweringService.AnswerAsync(testCase.Question, fixture.Chunks);

        await evaluator.AssertMeetsExpectationsAsync(testCase, answer);
    }
}
