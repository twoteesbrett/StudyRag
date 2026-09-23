# StudyRag — Course Material Cleaning and Markdown Conversion

## Objective

I am building StudyRag, a text-only retrieval-augmented generation (RAG) application for my NZ health and wellbeing course material.

I will upload ZIP files containing preprocessed HTML documents and associated images. Each ZIP represents one lesson.

Your task is to convert these into clean, accurate, text-only Markdown suitable for indexing and retrieval.

**This is a content-preservation and conversion task, NOT a summarisation task.**

## 1. Process the source material

For each ZIP file:

* Extract and examine the HTML and associated images.
* Identify the actual teaching material.
* Remove remaining Moodle navigation, technical clutter, decorative elements, irrelevant website controls and repetitive instructions such as "Click to expand".
* Preserve all substantive teaching content, including definitions, explanations, examples, scenarios, lists, tables, questions, answers and references.
* Preserve the original wording, terminology and level of detail wherever possible.
* Maintain the logical order and hierarchy of the material.
* Convert accordion content into appropriately headed Markdown sections.

Do not introduce information from outside the supplied material.

Do not silently correct, reinterpret, summarise or omit teaching content.

## 2. Convert images and diagrams to text

**The final output must be entirely text-based. No images or image dependencies are permitted.**

Examine every image and determine whether it contains meaningful information.

For meaningful diagrams:

* Transcribe all readable text.
* Preserve the relationships between concepts.
* Convert flowcharts into ordered processes with branches where appropriate.
* Convert cycles into clearly described sequences, including feedback loops.
* Convert models and frameworks into structured headings and bullet points.
* Convert charts into textual descriptions that retain the actual values and relationships shown.
* Convert tables saved as images into Markdown tables.

Preserve the original terminology and avoid changing the meaning.

Do not invent missing steps, labels, values or relationships.

If a diagram is ambiguous or illegible, preserve what can be established and flag the uncertainty in the processing report.

Remove purely decorative images.

Do not leave Markdown image references, image filenames or placeholders in the final teaching material.

## 3. Markdown formatting

Produce one Markdown file per lesson.

Use:

* A single H1 heading for the lesson title.
* H2 and H3 headings for the original content hierarchy.
* Ordinary paragraphs for explanations.
* Bullet points for unordered lists.
* Numbered lists for processes and ordered instructions.
* Markdown tables where appropriate.

Keep the structure clear and consistent without unnecessarily rewriting the material.

Avoid excessive formatting or artificial fragmentation.

Do not add your own explanations, interpretations, study tips or conclusions to the teaching material.

## 4. Preserve source accuracy

Use only information present in the uploaded files.

Do not use general knowledge or web research to fill gaps.

If a passage is unclear, preserve its original meaning as closely as possible.

If content was already removed by my preprocessor, such as H5P activities, videos or iframes, do not attempt to reconstruct it.

Document any known omissions in the processing report.

Do not mistake interactive controls or decorative SVG icons for meaningful teaching diagrams.

## 5. Quality checks

Before completing each lesson:

* Check that all substantive headings and sections have been retained.
* Check that paragraphs, definitions, examples and lists have not been accidentally omitted.
* Check that meaningful images have been converted into text.
* Check that the resulting Markdown contains no image references.
* Check that no information has been invented.
* Check that the final document contains no unnecessary Moodle navigation or technical clutter.

Compare the resulting Markdown against the preprocessed HTML, not merely against the extraction report.

Do not claim that validation passed unless it was actually performed.

## 6. Processing report

Produce a brief `PROCESSING_REPORT.md` documenting:

* Lessons processed.
* Significant content removed.
* Meaningful diagrams converted to text.
* Missing or ambiguous material.
* Known limitations, including previously removed embedded content.
* Any issues requiring my attention.

Keep the report concise. I do not need extensive statistics or unnecessary technical detail.

## 7. Deliverables

Return a single downloadable ZIP containing:

* One `.md` file per lesson.
* `PROCESSING_REPORT.md`.

Do not include images, HTML files or other assets.

Preserve lesson numbering and descriptive filenames.

Verify that the ZIP can be opened and contains all expected files.

## Important

My priority is accurate, complete teaching material for StudyRag, not aggressively reducing the word count.

**Preserve information first. Remove clutter second. Convert to Markdown third.**

Process the uploaded batch and provide the downloadable ZIP without asking for confirmation unless an essential part of the source material is genuinely missing.
