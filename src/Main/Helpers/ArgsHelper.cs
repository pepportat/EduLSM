using System.CommandLine;
using System.CommandLine.Help;

namespace Main.Helpers;

public class ProgramOptions
{
    public int MaxMemTableCount { get; set; }
    public required string DataPath { get; set; }
}

public static class ArgsHelper
{
    private const int DefaultMemTableCount = 15;
    
    public static ProgramOptions? ParseArgs(string[] args)
    {
        var defaultDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        Option<int> maxMemTableCount = new("--max-count")
        {
            Description = "Maximum count of the memtable.",
            DefaultValueFactory = _ => DefaultMemTableCount
        };
        
        Option<string> dataPath = new("--data-path")
        {
            Description = "Data directory for the SSTables.",
            DefaultValueFactory = _ => defaultDataPath
        };

        RootCommand rootCommand = new("LSM Engine")
        {
            maxMemTableCount,
            dataPath
        };

        var result = rootCommand.Parse(args);

        // Check for errors
        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
                Console.WriteLine(error.Message);

            // Show help manually
            rootCommand.Parse("--help").Invoke();
            return null;
        }

        // Check if user explicitly asked for help
        if (result.Tokens.Any(t => t.Value is "--help" or "-h"))
        {
            rootCommand.Parse("--help").Invoke();
            return null;
        }
        
        return new ProgramOptions
        {
            MaxMemTableCount = result.GetValue(maxMemTableCount),
            DataPath = result.GetValue(dataPath)!
        };
    }
}