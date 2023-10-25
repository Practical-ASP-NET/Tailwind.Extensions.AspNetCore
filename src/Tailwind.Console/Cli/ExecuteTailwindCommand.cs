using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Tailwind.Console.Cli;

public class ExecuteTailwindCommand : Command<ExecuteTailwindCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[TailwindCommand]")]
        public string? TailwindCommand { get; set; }
    }

    public override int Execute([NotNull]CommandContext context, [NotNull]Settings settings)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tailwindcli");

        // Get all files in the directory
        var files = Directory.GetFiles(path);

        // Check if there is exactly one file in the directory
        if (files.Length != 1)
        {
            System.Console.WriteLine($"Expected exactly one file in directory, but found {files.Length} files.");
            return -1;
        }

        // Get the full path of the file
        var filePath = files[0];

        // Create a new process start info

        var process = new Process();
        process.StartInfo = new ProcessStartInfo(filePath)
        {
            Arguments = settings.TailwindCommand
        };

        // Start the process
        process.Start();

        process.WaitForExit();

        // Return the exit code of the process
        return process.ExitCode;
    }
}