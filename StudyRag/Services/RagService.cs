using Microsoft.Extensions.AI;
using StudyRag.Helpers;
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
            context.Select(result =>
                $"[{result.Chunk.SourceFile}, chunk {result.Chunk.ChunkNumber}]\n{result.Chunk.Text}"));

        var prompt = $"""
            Answer the question using only the context below.
            Cite the supporting sources using their labels, for example [sample1.txt, chunk 4].

            Context:
            {contextText}

            Question:
            {question}
            """;

#if DEBUG
        DebugConsole.WriteHeader("PROMPT");
        DebugConsole.WriteLine(prompt);
#endif
        var response = await chatClient.GetResponseAsync(prompt);

        return response.Text;
    }
}
