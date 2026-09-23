## Roadmap

Current priority: use the prepared Markdown lessons with validated LLM-based chunking, then evaluate retrieval and answers against lesson-specific questions. Completed items below describe implemented capabilities, not a claim that live model evaluations have passed.

### 1. Document preparation pipeline

- [ ] Implement a DocumentPreparationService to prepare source material before indexing.
- [ ] Add support for Word (`.docx`) documents.
- [ ] Add support for PowerPoint (`.pptx`) presentations.
- [ ] Add support for PDF documents.
- [x] Implement initial HTML cleanup and extraction reports in the standalone preprocessor.
- [ ] Finish and verify removal of irrelevant content, formatting artefacts and duplicated text across the preparation workflow.
- [x] Draft a structured document model (`ExtractedDocument` and `DocumentBlock` types).
- [ ] Connect the document model to extraction and indexing; it is currently unused by the extractor.
- [ ] Preserve document structure, including headings, page numbers and slide references.
- [x] Investigate using an LLM to clean and structure extracted content (conversion prompt and prepared Markdown lessons).
- [x] Store prepared documents so they do not need to be processed again (saved Markdown for the manual workflow; automatic change detection remains below).

### 2. File tracking and change detection

- [ ] Generate a SHA-256 hash for each source file.
- [ ] Store file hashes and processing metadata in SQLite.
- [ ] Skip preparation and indexing when a file has not changed.
- [ ] Detect modified files and reprocess them automatically.
- [ ] Remove obsolete chunks and embeddings when a document is updated or deleted.
- [ ] Track processing status and errors.

### 3. Persistent storage and vector database

- [ ] Introduce SQLite for document metadata and processing history.
- [ ] Design tables for documents, source files and processing status.
- [ ] Introduce a local vector database for storing embeddings.
- [ ] Investigate Qdrant as a vector database.
- [ ] Replace in-memory similarity searching with vector database queries.
- [ ] Associate stored embeddings with their source documents and chunks.
- [ ] Ensure embeddings persist between application restarts.
- [ ] Implement an incremental indexing process.

### 4. Improve chunking and indexing

- [x] Split Markdown into bounded, overlapping request text before LLM chunking; validate each response independently.

- [x] Include prepared Markdown files in application output and indexing discovery.
- [x] Define the chunking input/output contract: numbered source blocks in, split-after IDs out; code constructs complete ranges.
- [ ] Complete the source metadata design, including heading hierarchy for each chunk.
- [x] Implement LLM-based selection of meaningful boundaries in the original text; construct chunk text in code rather than asking the model to rewrite it.
- [x] Validate selected boundaries for complete coverage, original order, valid references and size limits; reject invalid output without silently dropping content.
- [ ] Preview chunks from a representative lesson and check that explanations, examples, lists and tables retain their context.
- [ ] Evaluate LLM-based chunking with lesson-specific questions, checking retrieval separately from generated answers.

- [ ] Verify the chunking model's active context window and bound requests for each bounded request, including prompt and JSON overhead; the loaded qwen2.5 model reported 4,096 tokens during investigation.
- [ ] Verify the configured embedding model's token limit and enforce it before embedding, including any added heading context.
- [ ] Make the chunk size limit configurable; the current 4,000-character LLM limit is provisional, the legacy 1,000-character size / 100-character overlap splitter has been removed.
- [ ] Evaluate chunk size and overlap using course-specific test questions; compare retrieval quality with no overlap before deciding whether to add it.
- [ ] Preserve source metadata for accurate citations.
- [ ] Investigate whether overlapping chunks produce duplicate results.
- [ ] Support reindexing when the chunking configuration changes.
- [ ] Support reindexing when the embedding model changes.

### 5. Improve retrieval and evidence assessment

- [ ] Make retrieval settings configurable.
- [ ] Evaluate different similarity thresholds and candidate counts.
- [ ] Improve evidence assessment for questions requiring information from multiple chunks.
- [ ] Improve handling of questions that cannot be answered from the source material.
- [ ] Investigate hybrid retrieval combining keyword and vector searches.

### 6. Testing and evaluation

- [x] Add challenging evaluation cases for multiple passages, overlapping topics, incorrect premises and unsupported inference.
- [ ] Expand the evaluation dataset with questions from the prepared course lessons.
- [x] Add tests for missing details and partially answerable questions.
- [ ] Add tests for conflicting evidence.
- [x] Compare generated answers against expected answers (existing integration-test rubrics and LLM evaluator).
- [ ] Measure retrieval accuracy separately from answer quality.
- [ ] Investigate incorrect or unsupported citations.
- [ ] Add tests for file change detection and incremental indexing.

### 7. Code quality

- [ ] Complete the service and helper refactoring.
- [ ] Add XML documentation to public classes and methods.
- [ ] Improve error handling for Ollama connection failures and timeouts.
- [ ] Move configurable settings out of hard-coded values.
- [x] Improve logging and diagnostic output (progress at Information; counts, ranges, timings and decisions at Debug; full text at Trace).
- [x] Replace DebugConsole and DebugConsoleLoggerProvider with built-in console logging through ILogger; remove the custom diagnostic output and colour-handling code.