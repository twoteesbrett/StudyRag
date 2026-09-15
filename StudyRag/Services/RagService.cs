using Microsoft.Extensions.AI;
using StudyRag.Models;

namespace StudyRag.Services;

public class RagService(IChatClient chatClient)
{
    public async Task<string> AskAsync(
        string question,
        IEnumerable<RetrievalResult> context)
    {
        var contextText = string.Join(
            "\n\n",
            context.Select(result => result.Chunk.Text));

        var prompt = $"""
            Answer the question using only the context below.

            Context:
            {contextText}

            Question:
            {question}
            """;

        var response = await chatClient.GetResponseAsync(prompt);

        return response.Text;
    }
}