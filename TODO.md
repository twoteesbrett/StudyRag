# TODO

- [ ] Evaluate and tune retrieval relevance filtering using a small set of questions with expected matching chunks, including unrelated questions that should return no results. The Earth/Sun question scored 0.631 for the relevant chunk and 0.443/0.429 for unrelated chunks; the original 0.65 cutoff rejected the relevant result. The current 0.60 cutoff is provisional. Keep rejected scores visible during evaluation. Decide whether additional answer-support checks or retrieval retries are needed after measuring failures; similarity is not a confidence percentage.

- [x] Add evaluation mode: `dotnet run --project StudyRag -- --evaluate`. Prints all scores and compares cutoffs using expected sample1.txt chunks and unrelated questions.

Evaluation result (2026-09-18, configured nomic-embed-text):
- 0.60: precision 87.5%, recall 100%, exact expected sets 9/10, unrelated correctly empty 3/3.
- 0.65: precision 100%, recall 100%, exact expected sets 10/10, unrelated correctly empty 3/3.
- 0.70: recall falls to 85.7% (seasons question is rejected).
- Keep 0.60 provisional: this sample does not reproduce the previously reported 0.631 Earth/Sun result. Recover that exact question and add paraphrases and questions about covered topics whose answers are absent before raising the cutoff.
- No retrieval retries or answer-support checks added yet; the observed failure was an extra general RAG chunk for the embeddings question. Broader evaluation is needed to assess unsupported-answer risk.
