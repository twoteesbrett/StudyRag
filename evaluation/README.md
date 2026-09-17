# Overlap baseline

This is a synthetic software-class example, not user-provided class material. The four paragraphs in `StudyRag/Data/Evaluation/overlap.txt` cover transient, scoped, and singleton service lifetimes, plus general dependency injection. They are indexed only by the overlap evaluation; normal directory indexing does not recurse into this fixture folder.

Run from the repository root:

```powershell
dotnet run --project StudyRag -- --evaluate-overlap
```

Labels:
- New instance on every resolution: paragraph 1. The other lifetimes are related but do not satisfy that condition.
- Scoped versus singleton reuse across HTTP requests: paragraphs 2 and 3, both required.
- Bytes of memory per scoped instance: no supporting paragraph. None states a memory size.

Baseline on 2026-09-18 using the configured nomic-embed-text model, top 3 with a 0.60 cutoff:
- Single-paragraph question: retrieved 1, 2, 3; expected 1.
- Two-paragraph question: retrieved 3, 2, 1; expected 2 and 3.
- Missing-answer question: retrieved 2; expected no supporting evidence.
- Answer-support precision: 3/7 = 42.9%; recall: 3/3 = 100%.

All required evidence was retrieved, but extra topical passages were also selected. These labels measure direct answer support, not broad topical relevance. The missing-answer result does not prove that the generated answer would hallucinate; generation was not tested.

Full scores are in `overlap-2026-09-18.txt`. Per-chunk cutoff labels precede the top-3 limit; a chunk can pass the cutoff but fall outside the selected three.

Next: replace or supplement this synthetic fixture with a short real class extract, then compare a reranker against this baseline. Three questions establish the workflow, not reliable general performance. No cutoff or retrieval behavior was changed.
