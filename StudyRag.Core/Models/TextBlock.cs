namespace StudyRag.Core.Models;

/// <summary>A numbered passage of original text. IDs start at one.</summary>
public record TextBlock(int Id, string Text);
