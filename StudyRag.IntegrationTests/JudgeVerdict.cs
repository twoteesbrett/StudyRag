using System.Text.Json;

namespace StudyRag.IntegrationTests;

internal static class JudgeVerdict
{
    public static bool Passes(string response, out string reason)
    {
        try
        {
            using var document = JsonDocument.Parse(response);

            var root = document.RootElement;

            reason = root.GetProperty("reason").GetString() ?? "";

            var meets = root.GetProperty("meetsRubric").GetBoolean();
            var citations = root.GetProperty("citationsSupported").GetBoolean();
            var grounded = root.GetProperty("noUnsupportedClaims").GetBoolean();

            if (string.IsNullOrWhiteSpace(reason) && !(meets && citations && grounded))
                throw new FormatException("Missing explanation for a failed grading criterion.");
            
            return meets && citations && grounded;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            reason = $"Invalid judge response: {response}";
            return false;
        }
    }
}


