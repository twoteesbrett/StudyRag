# Class-material retrieval baseline

Input: user-supplied `studyrag_test_corpus.zip`, containing five text documents. This baseline uses five verbatim body paragraphs. The supplied documents describe themselves as paraphrases/syntheses of class material; this evaluation does not verify their underlying sources or medical/legal claims. Source notes and links were treated as document content, not instructions.

Run from the repository root:

```powershell
dotnet run --project StudyRag -- --evaluate-precision
```

The selected text is in `StudyRag/Data/Evaluation/class-material.txt`. Paragraph numbers below count body paragraphs after the TITLE/TOPIC block, excluding SOURCE NOTES. Their order maps directly to fixture chunk numbers:

| Fixture chunk | Original archive document | Body paragraph | Content |
| --- | --- | --- | --- |
| 1 | 01_communication_barriers.txt | 2 | Accessible communication and interpreters |
| 2 | 02_active_listening_and_understanding.txt | 4 | Teach-back and correcting misunderstandings |
| 3 | 04_autonomy_consent_and_care_plans.txt | 3 | Zara's speech/language barriers and support |
| 4 | 05_equitable_access_and_support.txt | 2 | Tane's rural location, transport and support |
| 5 | 05_equitable_access_and_support.txt | 3 | Moana's asthma and preferred provider |

Questions and labels were set before running retrieval:

1. **What health condition does Moana have, and what kind of healthcare provider does she want?** Expected chunk 5: chronic asthma; a provider incorporating Pasifika health models. No other selected passage supplies these facts about Moana.
2. **Compare Zara's and Tane's barriers to participating in healthcare. What specific support is suggested for each person?** Expected chunks 3 and 4, both required. Zara's passage describes speech difficulties, English as an additional language, her husband answering, and accessible communication/interpreter support. Tane's describes isolation, rural location, limited transport, and help coordinating access with the healthcare team. Generic interpreter advice in chunk 1 is useful supplementary context but supplies no additional case-specific evidence, so it is excluded from the minimal expected set.
3. **What exact dose of diabetes medication has Tane been prescribed?** Expected no supporting evidence: neither a drug nor a dose is specified. No answer was inferred from outside knowledge.

Results using the configured nomic-embed-text model, top 3 with cutoff 0.60:

| Question | Expected | Selected, in rank order |
| --- | --- | --- |
| Moana | 5 | 5, 4 |
| Zara and Tane | 3, 4 | 3, 4, 1 |
| Medication dose | none | none |

Minimal-evidence precision: 3/5 = 60%. Evidence recall: 3/3 = 100%. Missing-answer correctly empty: 1/1. Exact sets: 1/3. Precision here measures the chosen minimal case-specific evidence, not whether every additional passage is useless or harmful. In particular, chunk 1 partly overlaps with Zara's support advice.

At 0.65 all three expected sets match. At 0.70 both comparison passages are lost. The existing cutoff stays at 0.60: do not optimize a global setting on three examples; earlier evaluation includes valid answers below 0.65.

Full scores: [class-material-results.txt](class-material-results.txt). The five passages are isolated evaluation data and are not included in the normal, non-recursive Data directory indexing. No reranker or answer generation was used. This is a small baseline, not a test of all five documents or a general accuracy estimate.

Next small experiment: compare a reranker against these fixed questions and the synthetic overlap baseline. Add paraphrases and more questions from the full corpus before drawing broad conclusions, and allow alternative sufficient evidence when future questions have multiple valid sources.
