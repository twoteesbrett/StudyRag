# TODO

- [ ] Evaluate and tune retrieval relevance filtering using a small set of questions with expected matching chunks, including unrelated questions that should return no results. The Earth/Sun question scored 0.631 for the relevant chunk and 0.443/0.429 for unrelated chunks; the original 0.65 cutoff rejected the relevant result. The current 0.60 cutoff is provisional. Keep rejected scores visible during evaluation. Decide whether additional answer-support checks or retrieval retries are needed after measuring failures; similarity is not a confidence percentage.
