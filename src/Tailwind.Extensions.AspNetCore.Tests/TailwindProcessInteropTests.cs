using System.Runtime.InteropServices;

namespace Tailwind.Extensions.AspNetCore.Tests;

public class TailwindProcessInteropTests
{
    [Fact]
    public void StartProcess_ShouldRedirectStandardInput_ToAvoidConsoleHotkeyInterference()
    {
        // Arrange
        var interop = new TailwindProcessInterop();
       
        string fileName;
        string args;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName = "cmd"; 
            args = "/c echo hello";
        }
        else
        {
            fileName = "sh";
            args = "-lc 'echo hello'";
        }

        // Act
        using var process = interop.StartProcess(fileName, args);

        // Assert
        Assert.NotNull(process);
        Assert.True(process!.StartInfo.RedirectStandardInput, "Child process should not inherit console stdin");

        // Cleanup: ensure the process exits (it should be short-lived)
        process.WaitForExit(5000);
    }

    [Fact]
    public void ConfiguredPath_IsTheOnlyCandidate()
    {
        var candidates = TailwindProcessInterop.CandidateCommands("/opt/tools/tailwindcss", isWindows: false);

        Assert.Equal(new[] { "/opt/tools/tailwindcss" }, candidates);
    }

    [Fact]
    public void ConfiguredPathWithExtension_IsUsedAsGivenOnWindows()
    {
        var candidates = TailwindProcessInterop.CandidateCommands(@".\node_modules\.bin\tailwind.cmd", isWindows: true);

        Assert.Equal(new[] { @".\node_modules\.bin\tailwind.cmd" }, candidates);
    }

    [Fact]
    public void DefaultCommand_FallsBackOnlyToPublishedCliNames()
    {
        var candidates = TailwindProcessInterop.CandidateCommands(TailwindProcessInterop.DefaultCommand, isWindows: false).ToList();

        Assert.Equal(3, candidates.Count);
        Assert.Equal("tailwind", candidates[0]);
        Assert.Equal("tailwindcss", candidates[1]);
        Assert.Matches("^tailwindcss-(windows|macos|linux)-(x64|arm64)$", candidates[2]);
    }

    [Fact]
    public void DefaultCommand_TriesExeFirstOnWindows()
    {
        var candidates = TailwindProcessInterop.CandidateCommands(TailwindProcessInterop.DefaultCommand, isWindows: true).ToList();

        Assert.Equal(new[] { "tailwind.exe", "tailwind", "tailwindcss.exe", "tailwindcss" }, candidates.Take(4));
    }
}
