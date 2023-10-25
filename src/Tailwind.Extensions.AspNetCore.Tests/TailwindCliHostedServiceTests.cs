using FakeItEasy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tailwind.Extensions.AspNetCore.Tests;

public class TailwindCliHostedServiceTests
{
    [Fact]
    public async Task MissingOptionsShouldThrowException()
    {
        var tailwindOptions = new TailwindOptions
        {
            InputFile = "", // empty InputFile
            OutputFile = "", // empty OutputFile
            TailwindCliPath = null // null TailwindCliPath
        };
        var options = Options.Create(tailwindOptions);
        var tailwindProcess = A.Fake<ITailwindProcessInterop>();
        var hostEnvironment = A.Fake<IHostEnvironment>();
        var logger = A.Fake<ILogger<TailwindHostedService>>();

        A.CallTo(() => hostEnvironment.EnvironmentName).Returns(Environments.Development);
        var tailwindHostedService = new TailwindHostedService(options, hostEnvironment, tailwindProcess, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await tailwindHostedService.StartAsync(CancellationToken.None));
    }

    [Theory]
    [InlineData(null, "tailwind")]
    [InlineData("", "tailwind")]
    [InlineData("tailwindoverride.exe", "tailwindoverride.exe")]
    [InlineData("tailwindoverride", "tailwindoverride")]
    public async Task SpecifiedTailwindCliPathOverridesDefault(string tailwindCliPath, string expectedCliPath)
    {
        var tailwindOptions = new TailwindOptions
        {
            InputFile = "input.css", // empty InputFile
            OutputFile = "output.css", // empty OutputFile
            TailwindCliPath = tailwindCliPath // null TailwindCliPath
        };
        var options = Options.Create(tailwindOptions);
        var tailwindProcess = A.Fake<ITailwindProcessInterop>();
        var hostEnvironment = A.Fake<IHostEnvironment>();
        var logger = A.Fake<ILogger<TailwindHostedService>>();

        A.CallTo(() => hostEnvironment.EnvironmentName).Returns(Environments.Development);
        
        var tailwindHostedService = new TailwindHostedService(options, hostEnvironment, tailwindProcess, logger);
        await tailwindHostedService.StartAsync(CancellationToken.None);

        A.CallTo(() => tailwindProcess.StartProcess(expectedCliPath,A<string>.Ignored)).MustHaveHappened();
    }
}