using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    private readonly ITailwindProcessInterop _tailwindProcess;
    private readonly ILogger<TailwindHostedService> _logger;

    public TailwindHostedService(IOptions<TailwindOptions> options, IHostEnvironment hostEnvironment,
        ITailwindProcessInterop tailwindProcess, ILogger<TailwindHostedService> logger)
    {
        _logger = logger;
        _tailwindProcess = tailwindProcess;
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

        Guard.AgainstNull(input, "check Tailwind configuration");
        Guard.AgainstNull(output, "check Tailwind configuration");

        _logger.LogInformation($"tailwind -i {input} -o {output} --watch");

        var processName = string.IsNullOrEmpty(_options.TailwindCliPath) ? "tailwind" : _options.TailwindCliPath;
        _process = _tailwindProcess.StartProcess(processName, $"-i {input} -o {output} --watch");

        return Task.CompletedTask;
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