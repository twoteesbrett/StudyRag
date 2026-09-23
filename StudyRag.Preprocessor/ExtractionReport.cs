using System.Text;
using HtmlAgilityPack;

namespace StudyRag.Preprocessor;

public static class ExtractionReport
{
    public static string Create(string originalHtml, string retainedHtml)
    {
        var original = CountElements(originalHtml);
        var retained = CountElements(retainedHtml);
        var report = new StringBuilder();
        report.AppendLine("# Extraction report");
        report.AppendLine();
        report.AppendLine("Counts cover the whole input document, including hidden content and navigation.");
        report.AppendLine();
        report.AppendLine("| Content | Original | Retained | Change |");
        report.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var (name, count) in original)
        {
            var difference = retained[name] - count;
            report.AppendLine($"| {name} | {count} | {retained[name]} | {difference:+0;-0;0} |");
        }

        report.AppendLine();
        report.AppendLine($"Iframes removed: {original["Iframes"] - retained["Iframes"]}. Removed iframe content and references are excluded from the output.");
        report.AppendLine("Iframe counts cover elements in the main document; documents embedded in srcdoc are not inspected or included in the content counts.");
        report.AppendLine("Headings: h1–h6. Paragraphs: p. Tables: table. Images: img occurrences, not unique files.");
        report.AppendLine("Inline SVG diagrams are counted separately; images referenced only by CSS are not counted.");
        report.AppendLine("Accordions are an estimate: each details element or button (including role=button) with aria-expanded counts once. Controls inside details are excluded to avoid double counting. Other expandable controls may be included, and accordions without these semantics may be missed.");
        report.AppendLine();
        report.AppendLine("Matching counts do not guarantee that text, image contents or rendering are unchanged.");
        return report.ToString();
    }

    private static Dictionary<string, int> CountElements(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var counts = new Dictionary<string, int>
        {
            ["Headings"] = 0,
            ["Paragraphs"] = 0,
            ["Tables"] = 0,
            ["Accordions (estimated)"] = 0,
            ["Images"] = 0,
            ["Inline SVG diagrams"] = 0,
            ["Iframes"] = 0
        };

        foreach (var node in document.DocumentNode.Descendants())
        {
            switch (node.Name)
            {
                case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
                    counts["Headings"]++;
                    break;
                case "p": counts["Paragraphs"]++; break;
                case "table": counts["Tables"]++; break;
                case "img": counts["Images"]++; break;
                case "iframe": counts["Iframes"]++; break;
                case "svg": counts["Inline SVG diagrams"]++; break;
            }

            var isExpandableButton =
                (node.Name == "button" || node.GetAttributeValue("role", "")
                    .Equals("button", StringComparison.OrdinalIgnoreCase)) &&
                node.Attributes["aria-expanded"] is not null &&
                !node.Ancestors("details").Any();

            if (node.Name == "details" || isExpandableButton)
                counts["Accordions (estimated)"]++;
        }

        return counts;
    }
}
