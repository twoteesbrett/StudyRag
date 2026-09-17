# TODO

- [x] Evaluate retrieval relevance filtering with expected matching chunks, paraphrases, unrelated questions, and questions about covered topics whose answers are absent. Keep rejected scores visible and compare cutoffs before choosing further changes.
- [x] Add evaluation mode: `dotnet run --project StudyRag -- --evaluate`. Prints all scores, compares cutoffs, separates unrelated from unsupported-topic questions, and lists failures at 0.60 and 0.65.
- [ ] Add and evaluate answer-support/abstention handling: when retrieved text lacks the requested fact, the answer should explicitly say the documents do not provide it. Test actual generated answers against the unsupported questions before considering this solved.
- [ ] After answer-support handling, evaluate a lower cutoff or a controlled retrieval retry for missed paraphrases (for example, "Why do seasons change?"). Do not automatically relax filtering without checking unsupported answers.
- [ ] Extend evaluation beyond sample1.txt to the full document collection, identifying expected chunks by source file and chunk number.

Evaluation result (2026-09-18, configured nomic-embed-text, sample1.txt only):
- 33 questions: 18 answerable, 3 unrelated, 12 on-topic but unsupported by the text.
- Full scores and failures: [evaluation/retrieval-2026-09-18.txt](evaluation/retrieval-2026-09-18.txt).
- 0.55: precision 52.9%, recall 100%, exact expected sets 18/33, unrelated correctly empty 3/3, unsupported correctly empty 0/12.
- 0.60: precision 54.8%, recall 94.4%, exact expected sets 19/33, unrelated correctly empty 3/3, unsupported correctly empty 1/12.
- 0.65: precision 60.0%, recall 83.3%, exact expected sets 20/33, unrelated correctly empty 3/3, unsupported correctly empty 2/12.
- 0.70: precision 75.0%, recall 66.7%, exact expected sets 23/33, unrelated correctly empty 3/3, unsupported correctly empty 8/12.
- The user recovered the original question: "tell me about the earth and sun". It scores 0.628 for chunk 4 in this run, close to the earlier reported 0.631, and reproduces rejection at 0.65 and acceptance at 0.60.
- "Why do seasons change?" scores 0.583 for the correct chunk and is missed at 0.60. "Why can vectors help search for text with similar meaning?" scores 0.635 and is additionally missed at 0.65.
- An unsupported question, "What is Earth's orbital speed around the Sun?", scores 0.772 despite the document containing no speed. Topic similarity cannot establish answer support.
- Decision: retain 0.60 as a provisional retrieval cutoff. Raising it loses valid answers without reliably rejecting unsupported questions; lowering it recovers one answer but adds unsupported matches. Prioritize answer-support/abstention evaluation before retrieval retries.
- Precision here counts only answer-supporting chunks as correct, not merely topical matches. These results evaluate retrieval, not generated-answer correctness; no hallucination rate has been measured.
- This small hand-authored sample is diagnostic, not a general benchmark. Similarity is not a confidence percentage. No answer-support checks or retries were implemented in this evaluation task.

Small overlap baseline (2026-09-18):
- [x] Add four synthetic overlapping paragraphs and three labelled questions; run with `--evaluate-overlap`. See [baseline notes](evaluation/README.md) and [scores](evaluation/overlap-2026-09-18.txt).
- At 0.60, all required evidence is present (100% recall), but answer-support precision is 42.9%; the missing-answer question still retrieves a topical passage.
- [ ] Replace or supplement the synthetic fixture with real class material, then compare a reranker on passage precision and evidence recall. Do not tune the global cutoff to these three questions.

Class-material baseline:
- [x] Select five verbatim passages from the user-supplied studyrag_test_corpus.zip and label three questions before evaluation. Run with `--evaluate-class`; source mapping and rationale are in [class-material.md](evaluation/class-material.md).
- At 0.60: minimal case-specific evidence precision 60%, recall 100%, unsupported correctly empty 1/1. Additional generic advice can be useful context; this strict precision measure does not imply it is false.
- At 0.65: all three expected sets match; at 0.70: both comparison passages are missed. Keep 0.60 unchanged because this small sample does not override earlier failures.
- [ ] Compare a reranker on the fixed class-material and synthetic overlap baselines, then broaden the question set before choosing settings.
