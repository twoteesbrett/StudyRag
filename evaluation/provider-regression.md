# Provider-question regression

The user reported that `What healthcare provider does Moana want?` retrieved the right paragraph (similarity 0.719) but the model rejected it for not naming a provider. The text explicitly states the desired characteristic: incorporating Pasifika health models. That is sufficient for this question; the provider's name is a different, unsupported question.

Changes:
- Clarify that a description can answer a preference question without a proper name.
- Present numbered source sentences as plain text and ask for an evidence sentence ID, then resolve that ID to verbatim source text in code. This avoids false rejections caused by model-paraphrased quotations.
- Preserve partial evidence for comparisons. A rejection may reference an existing sentence while explaining an absence; its evidence is discarded and it remains unselected.
- Describe an empty selection as a reranker judgment that may be wrong, rather than proof that the documents contain no answer.
- Add both the reported question and the contrasting provider-name question to the existing evaluation.

Final-run transcript: [support-provider-final.txt](support-provider-final.txt). Local qwen2.5, temperature 0; unchanged nomic embeddings, cutoff 0.60 and top-three candidates. This is a development regression set used to revise the prompt, not an independent accuracy benchmark.

Class-material results (five questions): baseline precision 57.1%, recall 100%, exact 2/5; revised reranker precision 80%, recall 100%, exact 4/5; unsupported correctly empty 2/2; invalid assessments 0.

Synthetic overlap results (three questions): baseline precision 42.9%, recall 100%, exact 0/3; revised reranker precision 100%, recall 100%, exact 3/3; unsupported correctly empty 1/1; invalid assessments 0. Passage selection succeeds, although the chosen scoped-service evidence sentence is less specific than another sentence in the same retained passage.

The exact reported question now retains chunk 5; the provider-name question selects none. The original combined Moana question retains chunk 5, and the comparison retains both Zara and Tane. However, it also retains generic communication advice for that comparison, with an incorrect explanation attributing names to a passage that contains neither person. This remaining false positive means the scorer is still experimental. The model also labels full support as 2 rather than 3 in some cases; the ordinal levels are not calibrated confidence.

Earlier incomplete attempts are preserved in `support-provider-regression.txt`, `support-provider-regression-v2.txt`, `support-provider-regression-v3.txt`, and `support-provider-sentences.txt`. They exposed continued false rejection, paraphrased quotes, and a lost comparison passage. They are aborted diagnostic runs, not completed benchmarks. The final run restores evidence recall but does not establish perfect precision.

Validation: 21 unit tests pass, including malformed IDs, verbatim sentence resolution, valid rejected references, and retaining both comparison sides. Live evaluation tests the actual model judgments; mocked unit tests do not establish semantic accuracy.
