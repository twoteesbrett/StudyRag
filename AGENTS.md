
# StudyRag — Development Instructions

## Project overview

StudyRag is a C# retrieval-augmented generation (RAG) application using local Ollama models.

The goal is to answer questions using supplied study documents, with reliable evidence and source references.

## Architecture

The existing question-answering pipeline is:

1. Retrieve semantically similar document chunks.
2. Assess their relevance and supporting evidence.
3. Select appropriate context.
4. Generate an answer using the supplied context.

The planned indexing pipeline is:

1. Extract content from source documents.
2. Prepare and normalise extracted content.
3. Use an LLM to identify meaningful chunk boundaries.
4. Validate chunks against the original text.
5. Generate embeddings.
6. Store embeddings in a vector database.

Planned improvements include SQLite metadata storage, SHA-256 file hashing and incremental indexing.

These are plans, not necessarily implemented features.

## Development preferences

- Use C# and .NET.
- Prefer straightforward, readable code.
- Keep class and method names descriptive.
- Avoid unnecessary abstractions or design patterns.
- Maintain clear separation of responsibilities.

## How to work with me

This is a learning project. I want to understand and own the code.

- Explain the problem before proposing code.
- Make one small change at a time.
- Explain why a change is necessary.
- Do not implement large features without explicit permission.
- Do not refactor unrelated code.
- Ask before introducing additional dependencies.
- Prefer discussing design alternatives before implementation.
- Explain unfamiliar classes, methods and algorithms.
- When debugging, identify the underlying cause rather than merely making tests pass.
- Do not assume planned features have already been implemented.

## Current priority

Design the document extraction and preparation pipeline.

Before implementing it, establish a common document model that preserves source content, structure and metadata.

Avoid redesigning the existing prompt builder until the document model and extraction requirements are understood.