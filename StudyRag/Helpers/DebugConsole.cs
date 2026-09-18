namespace StudyRag.Helpers;

public static class DebugConsole
{
    public static void WriteLine(
        string message,
        ConsoleColor color = ConsoleColor.Green)
    {
        var originalColor = Console.ForegroundColor;

        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    public static void WriteHeader(string title)
    {
        WriteLine(
            $"\n=== {title} ===",
            ConsoleColor.Yellow);
    }
}