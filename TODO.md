## Roadmap

### 1. Document preparation pipeline

- [ ] Implement a DocumentPreparationService to prepare source material before indexing.
- [ ] Add support for Word (`.docx`) documents.
- [ ] Add support for PowerPoint (`.pptx`) presentations.
- [ ] Add support for PDF documents.
- [ ] Remove irrelevant content, formatting artefacts and duplicated text.
- [ ] Preserve document structure, including headings, page numbers and slide references.
- [ ] Investigate using an LLM to clean and structure extracted content.
- [ ] Store prepared documents so they do not need to be processed again.

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

- [ ] Evaluate chunk size and overlap using test questions.
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

- [ ] Expand the evaluation dataset with more challenging questions.
- [ ] Add tests for missing, conflicting and incomplete evidence.
- [ ] Compare generated answers against expected answers.
- [ ] Measure retrieval accuracy separately from answer quality.
- [ ] Investigate incorrect or unsupported citations.
- [ ] Add tests for file change detection and incremental indexing.

### 7. Code quality

- [ ] Complete the service and helper refactoring.
- [ ] Add XML documentation to public classes and methods.
- [ ] Improve error handling for Ollama connection failures and timeouts.
- [ ] Move configurable settings out of hard-coded values.
- [ ] Improve logging and diagnostic output.