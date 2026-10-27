using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tailwind;

public interface ITailwindProcessInterop
{
    Process? StartProcess(string command, string arguments);
}

public class TailwindProcessInterop : ITailwindProcessInterop
{
    public const string DefaultCommand = "tailwind";

    public Process? StartProcess(string command, string arguments)
    {
        try
        {
            var process = Start(arguments, CandidateCommands(command, RuntimeInformation.IsOSPlatform(OSPlatform.Windows)));

            if (process == null)
            {
                Console.WriteLine($"Could not start process: {command}");
                return null;
            }

            process.OutputDataReceived += (sender, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    Console.WriteLine("tailwind: " + data.Data);
                }
            };

            process.ErrorDataReceived += (sender, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    Console.WriteLine("tailwind: " + data.Data);
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting process: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// The commands to try, in order. A configured path is used as given; only the default command
    /// falls back to the other names the Tailwind CLI is published under, resolved by the OS from PATH.
    /// Nothing else on PATH or in the current directory is considered.
    /// </summary>
    internal static IEnumerable<string> CandidateCommands(string command, bool isWindows)
    {
        var names = new List<string> { command };
        if (command == DefaultCommand)
        {
            names.Add("tailwindcss");
            names.Add(StandaloneBinaryName());
        }

        foreach (var name in names)
        {
            if (isWindows && !Path.HasExtension(name))
                yield return $"{name}.exe";
            yield return name;
        }
    }

    // The name of the standalone CLI binary Tailwind publishes for this platform, e.g. tailwindcss-macos-arm64
    private static string StandaloneBinaryName()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos"
            : "linux";
        var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        return $"tailwindcss-{os}-{arch}";
    }

    private static Process? Start(string arguments, IEnumerable<string> commandsToTry)
    {
        var startInfo = new ProcessStartInfo
        {
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = null
        };
       
        foreach (var command in commandsToTry.Distinct())
        {
            try
            {
                startInfo.FileName = command;
                var process = Process.Start(startInfo);
                if (process == null) continue;
                
                Console.WriteLine("Started process: " + command);
                return process;
            }
            catch (Exception)
            {
                // Continue to next command variant
            }
        }

        return null;
    }

}