# StudyRag

StudyRag is a local, console-based retrieval-augmented generation (RAG) project for asking questions about your own text documents. It uses Ollama to create embeddings and generate answers, with an additional evidence-assessment step to check whether retrieved passages actually support the question.

The aim is to answer from supplied material, cite document passages, recognise missing information, and correct mistaken assumptions where the evidence allows. Evidence assessment and answer generation are LLM judgments, so their results can still be wrong.

## How it works

StudyRag has two main workflows: **indexing documents** when the application starts and **answering questions** during the interactive session.

### 1. Document indexing

```text
StudyRag/Data/**/*.txt
        |
        v
TextFileLoader.LoadAsync()
        |  Read each text file
        v
TextChunker
        |  Split text into smaller, overlapping (preserving context) chunks
        v
EmbeddingService.GenerateAsync()
        |  Generate an embedding for each chunk with Ollama
        v
IndexingService
        |  Create DocumentChunk records containing:
        |  source filename, chunk number, text and embedding
        v
In-memory collection of DocumentChunk records
```

`IndexingService.IndexDirectoryAsync()` finds `.txt` files recursively, loads and chunks each file, and generates an embedding for each chunk. The chunker splits at paragraph boundaries and further divides long paragraphs, preferring sentence or word boundaries when possible. Its defaults are a maximum of 1,000 characters and a 100-character overlap **within a long paragraph**.

The resulting chunks remain in memory for the current session. There is **no vector database or persisted index**: restarting the application indexes the files again.

### 2. Question answering

`QuestionAnsweringService.AnswerAsync()` coordinates the question-answering workflow:

```text
User question
     |
     v
RetrievalService.FindBestMatchesAsync()
     |  Embed the question
     |  Compare it with stored chunk embeddings using cosine similarity
     |  Keep up to 3 candidates with similarity >= 0.60
     v
EvidenceAssessmentService.AssessAsync()
     |  Ask the chat model to assess each candidate
     |  Score evidence (0-3), identify a supporting sentence,
     |  validate the response, and order the assessments
     v
Select evidence
     |  Keep valid passages scoring 2 or 3
     |
     +--------------------------+
     | Supporting passages      | No supporting passages
     v                          v
Use selected passages     Pass retrieved candidates as
                          related context, if any
     |                          |
     +-------------+------------+
                   v
             RagService.AskAsync()
                   |  Build a prompt with source labels
                   |  Ask Ollama to answer, cite facts, and
                   |  acknowledge gaps or correct assumptions
                   v
              QuestionAnswer
                   |  Answer text, supporting sources,
                   |  related sources and assessments
                   v
               Console output
```

The evidence scores are **ordinal judgments, not calibrated probabilities**:

| Score | Meaning |
| --- | --- |
| 0 | Unrelated passage. |
| 1 | Same topic, but no useful answer or case-specific evidence. |
| 2 | Supplies part of the answer or useful corrective/limiting context. |
| 3 | Directly supplies the requested information in full. |

The assessment also identifies a sentence from the original passage rather than inventing an evidence quotation. Only a **valid** assessment with a score of **2 or 3** is selected as supporting evidence. If none qualify, retrieved candidates may still be passed to the answer model as *related context* to help it identify mistaken premises or missing details; they are not treated as selected supporting sources.

The answer model receives document labels in the form `[filename.txt, chunk N]` and is instructed to cite document facts using those labels. General questions may receive clearly distinguished background information; specific claims about the supplied documents should be grounded in the passages. When there is no document evidence for a named person's circumstances, the model is instructed not to guess.

## Requirements

- .NET 10 SDK.
- A running, accessible [Ollama](https://ollama.com/) server.
- The `qwen2.5` chat model and `nomic-embed-text` embedding model (the configured defaults).

Download the default models:

```powershell
ollama pull qwen2.5
ollama pull nomic-embed-text
```

Check `StudyRag/Configuration/OllamaSettings.cs` before running. It defines the Ollama endpoint, model names and request timeout. The endpoint in the archived project points to a specific LAN address; change it to the address of **your** server, such as `http://localhost:11434` when Ollama runs on the same machine.

## Run

1. Put your `.txt` documents in `StudyRag/Data` (subdirectories are supported).
2. Make sure Ollama is running and the configured models are available.
3. From the solution directory, run:

```powershell
dotnet run --project StudyRag
```

The application indexes the documents on startup. Enter questions at the prompt and type `exit` to quit. There are no selectable answering modes.

## Solution structure

| Project | Responsibility |
| --- | --- |
| `StudyRag` | Console interaction, Ollama configuration, dependency injection and logging. |
| `StudyRag.Core` | Indexing, embeddings, retrieval, evidence assessment and answer generation. |
| `StudyRag.Tests` | Unit tests for code behaviour. |
| `StudyRag.IntegrationTests` | Live Ollama answer-quality tests with citation assertions and LLM grading. |
| `StudyRag.Evaluation` | A separate end-to-end evaluation command for manual review. |

Some important components:

| Component | Responsibility |
| --- | --- |
| `IndexingService` | Loads files, creates chunks and attaches embeddings and source metadata. |
| `TextFileLoader` | Reads text files. |
| `TextChunker` | Splits text into manageable, overlapping chunks. |
| `EmbeddingService` | Requests embeddings from Ollama. |
| `RetrievalService` | Selects candidates by cosine similarity. |
| `VectorMath` | Calculates cosine similarity between embedding vectors. |
| `EvidenceAssessmentService` | Asks the chat model to assess the evidence and orders the assessments. |
| `QuestionAnsweringService` | Coordinates retrieval, evidence selection and answer generation. |
| `RagService` | Prompts the chat model and returns the generated answer. |

`TextFileLoader`, `TextChunker` and `VectorMath` are organised under `StudyRag.Core/Helpers` in the proposed refactor. If you are using the original ZIP unchanged, these three files are still under `StudyRag.Core/Services`, and the evidence-assessment class and method are named `EvidenceReranker.RankAsync()` instead.

## Tests and evaluation

Run the automated tests:

```powershell
dotnet test StudyRag.slnx
```

The integration tests require a live Ollama server. To enable them in PowerShell:

```powershell
$env:STUDYRAG_INTEGRATION_TESTS = '1'
dotnet test StudyRag.slnx
```

Without that environment variable, live integration tests are skipped. The integration suite contains eleven answer-quality cases and uses citation assertions and LLM grading.

For the separate evaluation command:

```powershell
dotnet run --project StudyRag.Evaluation
```

See [evaluation details](evaluation/README.md). The synthetic overlapping-content fixture in `StudyRag.Evaluation/Data` is used for evaluation and is not part of normal chat data.

## Current scope and limitations

- **Text input only:** indexing currently discovers `.txt` files; Word, PDF and PowerPoint extraction is not implemented.
- **Memory only:** chunks and embeddings are rebuilt on each application start; no vector store or persistent cache is used.
- **Fixed retrieval settings:** question answering currently requests up to three candidates at a similarity threshold of 0.60.
- **Model-dependent checks:** similarity, evidence scores and generated citations can be misleading or incorrect. A score of 3 does not guarantee a correct final answer.
- **Source quality matters:** irrelevant text, poorly extracted documents or missing context can reduce retrieval and answer quality.

This is a learning project and an evolving RAG pipeline, not a guarantee of factual correctness.
