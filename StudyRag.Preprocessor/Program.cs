using System.IO.Compression;

using StudyRag.Preprocessor;

//var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));

//var inputPath = Path.Combine(projectDirectory, "Input");
//var outputPath = Path.Combine(projectDirectory, "Output");

var inputPath = "C:\\Users\\270983191\\OneDrive - UP Education\\Documents\\Module 1\\UpLearn Materials Module 1";
var outputPath = Path.Combine(inputPath, "Output");

// Start each run with an empty output folder.
var outputDirectory = Directory.CreateDirectory(outputPath);

foreach (var file in outputDirectory.EnumerateFiles())
{
    file.Delete();
}
foreach (var directory in outputDirectory.EnumerateDirectories())
{
    directory.Delete(recursive: true);
}

// Extract the HTML content from each input file and save it to a temporary directory, then archive it into a zip file in the output folder.
var extractor = new HtmlExtractor();

foreach (var inputFile in Directory.EnumerateFiles(inputPath, "*.html"))
{
    var temporaryDirectory = Directory.CreateTempSubdirectory("StudyRag-Preprocessor-");
    var documentOutputPath = Path.Combine(temporaryDirectory.FullName, "document");
    Directory.CreateDirectory(documentOutputPath);

    try
    {
        var html = await File.ReadAllTextAsync(inputFile);
        var extractedHtml = extractor.Extract(html, documentOutputPath, extractImages: true);

        var outputFile = Path.Combine(documentOutputPath, Path.GetFileName(inputFile));
        await File.WriteAllTextAsync(outputFile, extractedHtml);

        var reportFile = Path.Combine(documentOutputPath, "extraction-report.md");
        await File.WriteAllTextAsync(reportFile, ExtractionReport.Create(html, extractedHtml));

        // Finish the archive before moving it into the output folder.
        var temporaryZipPath = Path.Combine(temporaryDirectory.FullName, "document.zip");
        ZipFile.CreateFromDirectory(documentOutputPath, temporaryZipPath);

        var zipPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(inputFile) + ".zip");
        File.Move(temporaryZipPath, zipPath, overwrite: true);
        Console.WriteLine($"Extracted document saved to: {zipPath}");

        foreach (var warning in extractor.Warnings)
        {
            Console.WriteLine($"Warning: {warning}");
        }
    }
    finally
    {
        temporaryDirectory.Delete(recursive: true);
    }
}
