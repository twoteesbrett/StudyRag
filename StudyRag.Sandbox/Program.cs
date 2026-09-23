using MarkItDown;
using MimeKit;

var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));

var inputPath = Path.Combine(projectDirectory, "Input", "Course 1.mhtml");
var outputPath = Path.Combine(projectDirectory, "Output", "Course 1.md");

// Extract HTML from the MHTML archive.
using var stream = File.OpenRead(inputPath);

var message = MimeMessage.Load(stream);

var html = message.BodyParts
    .OfType<TextPart>()
    .FirstOrDefault(part => part.IsHtml)
    ?.Text;

if (string.IsNullOrWhiteSpace(html))
{
    throw new InvalidOperationException(
        "No HTML content found in MHTML file.");
}

// Convert HTML to Markdown.
var client = new MarkItDownClient();

await using var htmlStream = new MemoryStream(
    System.Text.Encoding.UTF8.GetBytes(html));

var info = new StreamInfo(
    extension: ".html",
    mimeType: "text/html");

await using var result = await client.ConvertAsync(
    htmlStream,
    info);

// Save Markdown.
await File.WriteAllTextAsync(
    outputPath,
    result.Markdown);