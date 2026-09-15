using Microsoft.Extensions.AI;
using OllamaSharp;
using StudyRag.Models;
using StudyRag.Services;

var ollamaUri = new Uri("http://192.168.1.11:11434");

IChatClient chatClient =
    new OllamaApiClient(
        ollamaUri,
        "qwen2.5");

IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    new OllamaApiClient(
        ollamaUri,
        "nomic-embed-text");

var embeddingService =
    new EmbeddingService(embeddingGenerator);

var retrievalService =
    new RetrievalService(embeddingService);

var ragService =
    new RagService(chatClient);

var textFileLoader = new TextFileLoader();
var textChunker = new TextChunker();

var text = await textFileLoader.LoadAsync(
    "Data/sample.txt");

var documentTexts = textChunker.Chunk(text);

var chunks = new List<DocumentChunk>();

foreach (var documentText in documentTexts)
{
    var embedding =
        await embeddingService.GenerateAsync(documentText);

    chunks.Add(new DocumentChunk {
        Text = documentText,
        Embedding = embedding
    });
}

var question =
    "What is dependency injection?";

var matches =
    await retrievalService.FindBestMatchesAsync(
        question,
        chunks,
        count: 2);

var answer =
    await ragService.AskAsync(
        question,
        matches);

Console.WriteLine(answer);