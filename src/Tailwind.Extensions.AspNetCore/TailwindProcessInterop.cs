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
            var startInfo = new ProcessStartInfo
            {
                FileName = cmdName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);

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
                    Console.WriteLine("tailwind: " +data.Data);
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
}