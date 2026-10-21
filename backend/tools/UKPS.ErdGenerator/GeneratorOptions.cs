namespace UKPS.ErdGenerator;

/// <summary>Parsed command-line options.</summary>
internal sealed record GeneratorOptions(
    string GroupsPath,
    string OutputPath,
    string? HtmlPath,
    bool Check
)
{
    public static GeneratorOptions? Parse(string[] args)
    {
        string? groupsPath = null;
        string? outputPath = null;
        string? htmlPath = null;
        bool check = false;

        Queue<string> queue = new(args);
        while (queue.Count > 0)
        {
            string arg = queue.Dequeue();
            switch (arg)
            {
                case "--groups" when queue.Count > 0:
                    groupsPath = queue.Dequeue();
                    break;
                case "--output" when queue.Count > 0:
                    outputPath = queue.Dequeue();
                    break;
                case "--html" when queue.Count > 0:
                    htmlPath = queue.Dequeue();
                    break;
                case "--check":
                    check = true;
                    break;
                default:
                    Console.Error.WriteLine($"Unrecognised argument: {arg}");
                    return null;
            }
        }

        if (groupsPath is null || outputPath is null)
        {
            return null;
        }

        return new GeneratorOptions(groupsPath, outputPath, htmlPath, check);
    }

    public static void PrintUsage()
    {
        Console.Error.WriteLine(
            "Usage: UKPS.ErdGenerator --groups <erd-groups.yml> --output <erd.mmd> [--html <erd.html>] [--check]"
        );
        Console.Error.WriteLine();
        Console.Error.WriteLine(
            "  --groups   Path to the hand-maintained logical grouping config."
        );
        Console.Error.WriteLine("  --output   Path of the generated Mermaid ERD.");
        Console.Error.WriteLine(
            "  --html     Optional path of the generated interactive HTML viewer."
        );
        Console.Error.WriteLine(
            "  --check    Do not write; exit 1 if the committed ERD differs from the model."
        );
    }
}
