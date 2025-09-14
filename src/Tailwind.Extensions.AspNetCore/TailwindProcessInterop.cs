using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tailwind;

public interface ITailwindProcessInterop
{
    Process? StartProcess(string command, string arguments);
}

public class TailwindProcessInterop : ITailwindProcessInterop
{
    public Process? StartProcess(string command, string arguments)
    {
        string cmdName;
        if (!command.EndsWith(".exe") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            cmdName = $"{command}.exe";
        }
        else
            cmdName = command;

        try
        {
            var process = Start(arguments, cmdName);

            if (process == null)
            {
                Console.WriteLine($"Could not start process: {cmdName}");
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

    private static IEnumerable<string> FindExecutablesInPath(string pattern)
    {
        var executables = new List<string>();
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv)) return executables;
        var pathDirs = pathEnv.Split(Path.PathSeparator);
        foreach (var dir in pathDirs)
        {
            if (!Directory.Exists(dir)) continue;
            try
            {
                var files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        continue;
                    executables.Add(file);
                }
            }
            catch { /* ignore access issues */ }
        }
        return executables;
    }

    private static IEnumerable<string> FindExecutablesInCurrentDirectory(string pattern)
    {
        var executables = new List<string>();
        var dir = Directory.GetCurrentDirectory();
        try
        {
            var files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    continue;
                executables.Add(file);
            }
        }
        catch { /* ignore access issues */ }
        return executables;
    }

    private static Process? Start(string arguments, string cmdName)
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
       
        var commandsToTry = new List<string>();
       
        commandsToTry.AddRange(FindExecutablesInPath("*tailwind*"));
        commandsToTry.Add(cmdName);
        commandsToTry.AddRange(FindExecutablesInCurrentDirectory(cmdName));
      
        if (cmdName.EndsWith(".exe"))
        {
            var baseCommand = cmdName.Substring(0, cmdName.Length - 4);
            commandsToTry.Add(baseCommand);
            commandsToTry.AddRange(FindExecutablesInCurrentDirectory(baseCommand));
        }
      
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