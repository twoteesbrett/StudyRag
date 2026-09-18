# Evaluation commands

Run `dotnet run --project StudyRag -- --help` for command help.

| Option | Data and purpose |
| --- | --- |
| `--evaluate-sample-retrieval` | 33 sample1.txt questions; compare similarity cutoffs. Previously `--evaluate`. |
| `--evaluate-precision` | Five passages from your uploaded class material; measure retrieval precision and recall. Previously `--evaluate-class`. |
| `--evaluate-synthetic-overlap` | Four synthetic service-lifetime paragraphs. Previously `--evaluate-overlap`. |
| `--evaluate-reranking` | Compare unchanged top-3/0.60 retrieval with evidence-support reranking on both overlap fixtures. |
| `--rerank` | Interactive questions with the experimental reranker. |

Old names are no longer accepted. These evaluations measure passage selection, not generated-answer correctness.

The reranker uses the configured qwen2.5 chat model to judge each candidate independently:
0 = unrelated; 1 = topical only; 2 = direct partial support; 3 = direct full support.
It retains scores 2 and 3, sorted by support then similarity. Partial support is retained so comparisons can use multiple passages.
Every retained result needs a valid source-sentence ID. The code copies that sentence verbatim from the passage; malformed responses and invalid IDs are excluded with a diagnostic reason.
An exact quote establishes provenance, not whether the model correctly judged its relevance. Scores are not calibrated confidence percentages.
The candidate pool and cutoff stay unchanged to isolate selection precision; this step cannot recover evidence missed by retrieval.
The feature remains opt-in pending broader evaluation. Each candidate adds a local model call.

Completed comparison and limitations: [reranking.md](reranking.md).
The later Moana false-rejection fix and remaining precision limitation are documented in [provider-regression.md](provider-regression.md).

# Synthetic overlap baseline

This is a synthetic software-class example, not user-provided class material. The four paragraphs in `StudyRag/Data/Evaluation/overlap.txt` cover transient, scoped, and singleton service lifetimes, plus general dependency injection. They are indexed only by the overlap evaluation; normal directory indexing does not recurse into this fixture folder.

Run from the repository root:

```powershell
dotnet run --project StudyRag -- --evaluate-synthetic-overlap
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
