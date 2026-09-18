# StudyRag

An experimental console RAG application using local Ollama models. Answers combine general knowledge with cited document context; optional evidence reranking selects which retrieved passages to include.

## Projects

| Project | Responsibility |
| --- | --- |
| `StudyRag` | Console interaction, command parsing, Ollama configuration, and DI registration. |
| `StudyRag.Core` | Document indexing, embeddings, retrieval, evidence reranking, and answer generation. No console or Ollama dependency. |
| `StudyRag.Evaluation` | Retrieval and reranking evaluation fixtures and reporting. Depends on Core. |
| `StudyRag.Tests` | Core behaviour, command routing, and DI wiring tests. |

`Program.cs` is the composition root. `Configuration/ServiceRegistration.cs` registers the services and owns client lifetimes through the DI container. `Configuration/OllamaSettings.cs` contains the endpoint, model names, and ten-minute per-request timeout. Change those settings to use another Ollama instance.

`ConsoleApp/ChatSession.cs` runs interactive questions; `ConsoleApp/EvaluationRunner.cs` dispatches evaluation commands. Core services accept their dependencies through constructors. They use standard logging; the app supplies coloured console output.

The former support classes are now `EvidenceReranker`, `EvidenceAssessment`, and `RerankingEvaluation`. The rename and project separation do not change the scoring rubric or prompt behaviour.

## Running

Start Ollama with the `qwen2.5` and `nomic-embed-text` models available, then:

```powershell
dotnet run --project StudyRag
dotnet run --project StudyRag -- --rerank
dotnet run --project StudyRag -- --help
dotnet test StudyRag.slnx
```

Text files in `StudyRag/Data` are copied to the output directory and indexed recursively, including the evaluation subdirectory. Empty context still permits general-background answers. Facts about supplied cases must come from the documents.

See [evaluation commands and limitations](evaluation/README.md). Evidence scores are experimental model judgments, not confidence probabilities.
