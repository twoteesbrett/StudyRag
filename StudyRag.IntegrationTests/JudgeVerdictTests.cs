namespace StudyRag.IntegrationTests;

public class JudgeVerdictTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("not JSON")]
    [InlineData("null")]
    [InlineData("{\"meetsRubric\":true,\"citationsSupported\":true,\"noUnsupportedClaims\":true}")]
    [InlineData("{\"meetsRubric\":\"true\",\"citationsSupported\":true,\"noUnsupportedClaims\":true,\"reason\":\"OK\"}")]
    public void Malformed_grades_fail_closed(string response) => Assert.False(JudgeVerdict.Passes(response, out _));

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Every_criterion_must_pass(bool facts, bool citations, bool grounded)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            meetsRubric = facts,
            citationsSupported = citations,
            noUnsupportedClaims = grounded,
            reason = "Grading explanation"
        });

        Assert.Equal(facts && citations && grounded, JudgeVerdict.Passes(json, out var reason));
        Assert.Equal("Grading explanation", reason);
    }
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    public void Empty_explanation_is_allowed_only_when_every_check_passes(
        bool facts, bool citations, bool grounded, bool expected)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            meetsRubric = facts,
            citationsSupported = citations,
            noUnsupportedClaims = grounded,
            reason = ""
        });

        Assert.Equal(expected, JudgeVerdict.Passes(json, out _));
    }
}
