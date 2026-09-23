namespace StudyRag.Preprocessor.Models;

public record SourceLocation
{
    /// <summary>A one-based page number, when the source has pages.</summary>
    public int? PageNumber { get; init; }

    /// <summary>A format-specific locator, such as an HTML XPath.</summary>
    public string? ElementPath { get; init; }
}

public abstract record DocumentBlock
{
    public SourceLocation? Location { get; init; }
}

public record HeadingBlock(int Level, string Text) : DocumentBlock;

public record ParagraphBlock(string Text) : DocumentBlock
{
    public IReadOnlyList<DocumentLink> Links { get; init; } = [];
}

/// <summary>The original target may be a relative path or an absolute URL.</summary>
public record DocumentLink(string Text, string Target);

/// <summary>Items contain blocks so nested lists and paragraphs remain structured.</summary>
public record ListBlock(bool IsOrdered, IReadOnlyList<ListItem> Items) : DocumentBlock
{
    public int StartNumber { get; init; } = 1;
}

public record ListItem(IReadOnlyList<DocumentBlock> Blocks);

public record TableBlock(IReadOnlyList<TableRow> Rows) : DocumentBlock
{
    public string? Caption { get; init; }
}

public record TableRow(IReadOnlyList<TableCell> Cells);

public record TableCell(IReadOnlyList<DocumentBlock> Blocks)
{
    public bool IsHeader { get; init; }
    public int RowSpan { get; init; } = 1;
    public int ColumnSpan { get; init; } = 1;
}

/// <summary>Retains the original image reference, including paths or data URIs.</summary>
public record ImageBlock(string Source, string? AlternativeText) : DocumentBlock
{
    public string? Caption { get; init; }
}
