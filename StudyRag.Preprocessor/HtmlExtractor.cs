using HtmlAgilityPack;

namespace StudyRag.Preprocessor;

public class HtmlExtractor
{
    public List<string> Warnings { get; } = [];

    public string Extract(string html, string outputDirectory, bool extractImages = false)
    {
        Warnings.Clear();

        var document = new HtmlDocument();
        document.LoadHtml(html);

        // Remove iframe elements together with their embedded content and references.
        foreach (var node in document.DocumentNode.Descendants().ToArray())
        {
            // SVG styling can be essential to a diagram's meaning.
            var isSvgContent = node.Name == "svg" || node.Ancestors("svg").Any();
            var isStylesheet = node.Name == "link" &&
                node.GetAttributeValue("rel", "").Split((char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries)
                .Contains("stylesheet", StringComparer.OrdinalIgnoreCase);

            if (node.NodeType == HtmlNodeType.Comment || node.Name is "script" or "iframe" ||
                (node.Name == "style" && !isSvgContent) || isStylesheet)
            {
                node.Remove();
                continue;
            }

            foreach (var attribute in node.Attributes.ToArray())
            {
                if (attribute.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                    (attribute.Name == "style" && !isSvgContent))
                {
                    node.Attributes.Remove(attribute);
                }
            }
        }

        // Leave image references unchanged unless file extraction is requested.
        if (extractImages)
        {
            // Keep images, alternative text, captions and their positions in the page.
            foreach (var image in document.DocumentNode.Descendants("img").ToArray())
            {
                var sourceReference = HtmlEntity.DeEntitize(image.GetAttributeValue("src", ""));
                if (!sourceReference.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    Warnings.Add($"Image reference retained but not copied: {image.GetAttributeValue("alt", "Unlabelled image")}");
                    continue;
                }
    
                try
                {
                    image.SetAttributeValue("src", EmbeddedImageSaver.Save(sourceReference, outputDirectory));
                }
                catch (FormatException exception)
                {
                    Warnings.Add($"Image kept embedded: {exception.Message}");
                }
            }
        }

        return document.DocumentNode.OuterHtml;
    }
}
