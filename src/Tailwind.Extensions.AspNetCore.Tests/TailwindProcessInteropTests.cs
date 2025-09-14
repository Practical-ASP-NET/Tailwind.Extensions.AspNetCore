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
}
