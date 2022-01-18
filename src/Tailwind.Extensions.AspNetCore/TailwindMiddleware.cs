// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.AspNetCore.NodeServices.Util;
using Microsoft.AspNetCore.SpaServices.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tailwind;

internal static class TailwindMiddleware
{
    private const string LogCategoryName = "NodeServices";

    private static readonly TimeSpan
        RegexMatchTimeout =
            TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

    public static async Task Attach(IApplicationBuilder appBuilder, string scriptName)
    {
        var applicationStoppingToken = appBuilder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStopping;
        var logger = LoggerFinder.GetOrCreateLogger(appBuilder, LogCategoryName);
        var diagnosticSource = appBuilder.ApplicationServices.GetRequiredService<DiagnosticSource>();
        await ExecuteScript("./", scriptName, logger, diagnosticSource, applicationStoppingToken);
    }

    private static async Task ExecuteScript(
        string sourcePath, string scriptName, ILogger logger,
        DiagnosticSource diagnosticSource, CancellationToken applicationStoppingToken)
    {
        var envVars = new Dictionary<string, string>() { };

        var scriptRunner = new NodeScriptRunner(
            sourcePath, scriptName, null, envVars, "npm", diagnosticSource, applicationStoppingToken);
        scriptRunner.AttachToLogger(logger, true);

        using var stdErrReader = new EventedStreamStringReader(scriptRunner.StdErr);
        try
        {
            await scriptRunner.StdOut.WaitForMatch(
                new Regex("Done in", RegexOptions.None, RegexMatchTimeout));
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidOperationException(
                $"The npm script '{scriptName}' exited without indicating that the " +
                "Tailwind CLI had finished. The error output was: " +
                $"{stdErrReader.ReadAsString()}", ex);
        }
    }
}