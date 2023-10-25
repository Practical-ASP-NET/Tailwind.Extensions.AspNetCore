using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Tailwind;

public class TailwindOptions
{
    public string? InputFile { get; set; }
    public string? OutputFile { get; set; }
    public string? TailwindCliPath { get; set; }
}

/// <summary>
/// Launches the Tailwind CLI in watch mode (if the CLI is installed and exists in the system Path)
/// Runs as a background service (will be shut down when the app is stopped)
/// Only works in development environments
/// </summary>
public class TailwindHostedService : IHostedService, IDisposable
{
    private Process? _process;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly TailwindOptions _options;

    public TailwindHostedService(IOptions<TailwindOptions> options, IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // don't want this running in production, only useful for development time generation of the Tailwind file
        // use another mechanism such as CI/CD to generate the production-ready CSS style sheet
        if (!_hostEnvironment.IsDevelopment())
            return Task.CompletedTask;

        
        var input = _options.InputFile;
        var output = _options.OutputFile;

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
        {
            Console.WriteLine("tailwind: Unable to start Tailwind CLI, missing CSS input and output file config. Check your app configuration.");
        }
        
        Console.WriteLine($"tailwind -i {input} -o {output} --watch");

        var processName = string.IsNullOrEmpty(_options.TailwindCliPath) ? "tailwind" : _options.TailwindCliPath;
        _process = StartProcess(processName, $"-i {input} -o {output} --watch");

        return Task.CompletedTask;
    }

    private static Process? StartProcess(string command, string arguments)
    {
        var cmdName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"{command}.exe"
            : command;

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _process?.Kill();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _process?.Dispose();
    }
}