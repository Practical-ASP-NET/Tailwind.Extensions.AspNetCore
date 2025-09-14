Project: Tailwind.Extensions.AspNetCore
Last updated: 2025-09-08

Scope
- Library: src/Tailwind.Extensions.AspNetCore (ASP.NET Core extension to run Tailwind CLI in watch mode during Development)
- Tests: src/Tailwind.Extensions.AspNetCore.Tests (xUnit)
- Sample app: src/SampleApps/BlazorServer (demonstrates usage)
- Build/pack scripts: Build.ps1, Push.ps1

Build and configuration instructions
1) Requirements
   - .NET SDK: project targets net6.0. A .NET 6 Runtime is required to run tests and sample apps; building the code with newer SDKs works but running requires a matching runtime.
     • Verified: Building with a .NET 10 preview SDK succeeds; running tests failed due to missing Microsoft.NETCore.App 6.0 runtime. Install .NET 6 Runtime when executing tests: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
   - Windows PowerShell (Build.ps1 uses PowerShell).
   - For the sample Blazor app only: Node.js + npm to install and run Tailwind CLI locally.

2) Build via CLI
   - Restore and build everything:
     dotnet build -c Release
   - Scripted build (cleans, builds, tests, packs):
     PowerShell: ./Build.ps1
     Behavior of Build.ps1:
       • Cleans solution in Release.
       • Builds in Release.
       • Runs tests in Release, outputs TRX to ./artifacts.
       • Packs the library project to ./artifacts.

3) Packaging
   - Create a NuGet package (Release):
     dotnet pack .\src\Tailwind.Extensions.AspNetCore -c Release -o .\artifacts --no-build
   - Push.ps1 is available for publishing (inspect/update it as needed for your feed credentials).

Testing information
1) Test framework and structure
   - Framework: xUnit with Microsoft.NET.Test.Sdk and coverlet.collector.
   - Location: src/Tailwind.Extensions.AspNetCore.Tests
   - GlobalUsings includes Xunit, so test files can omit using Xunit.

2) Running tests
   - Prerequisite: .NET 6 runtime must be installed to execute tests (TargetFramework=net6.0). Without it, dotnet test will abort with a message about missing Microsoft.NETCore.App 6.0.
   - Commands:
     • All tests, Debug:
       dotnet test -c Debug
     • All tests, Release, with TRX logs to ./artifacts (no rebuild):
       dotnet test -c Release -r .\artifacts --no-build -l trx
     • Single project:
       dotnet test .\src\Tailwind.Extensions.AspNetCore.Tests\Tailwind.Extensions.AspNetCore.Tests.csproj -c Debug

3) Adding new tests
   - Create a new .cs file under src/Tailwind.Extensions.AspNetCore.Tests with [Fact]/[Theory] tests.
   - Example (minimal):
       using Tailwind; // if testing internals in the library
       public class ExampleTests {
           [Fact]
           public void Sanity() => Assert.True(true);
       }
   - If you need fakes/mocks, FakeItEasy is already referenced.

4) Demo test run (validated process)
   - We added a trivial xUnit test file and compiled successfully. Running tests failed on this machine because the .NET 6 runtime is not installed. This confirms:
     • The test project compiles and is discovered by dotnet test.
     • Execution requires Microsoft.NETCore.App 6.0. Install it to run tests locally, or multi-target the test project to a runtime available on your machine.

Guidance for multi-targeting tests (optional for local dev)
- If your environment only has newer runtimes (e.g., .NET 10 preview) and you need to execute tests without installing .NET 6, you can temporarily multi-target the test project:
  • Change <TargetFramework> to <TargetFrameworks>net6.0;net10.0</TargetFrameworks> in src/Tailwind.Extensions.AspNetCore.Tests.csproj.
  • Ensure Microsoft.NET.Test.Sdk and xunit packages support the newer TFM (you may need newer package versions when moving beyond LTS). Run: dotnet restore, then dotnet test -f net10.0.
  • Revert this change before committing if you want to keep the official target at net6.0.

Additional development information
1) Library responsibilities (TailwindHostedService)
   - Namespace Tailwind; primary types:
     • TailwindOptions: InputFile, OutputFile, TailwindCliPath.
     • TailwindHostedService: IHostedService that spawns the Tailwind CLI in watch mode during Development only.
     • ITailwindProcessInterop and TailwindProcessInterop: process abstraction to start CLI (StartProcess(name, args)).
   - Behavior:
     • Service is a no-op outside Development (IHostEnvironment.IsDevelopment()).
     • Validates InputFile/OutputFile via Guard.AgainstNull (throws ArgumentNullException with context message).
     • Chooses CLI executable: TailwindCliPath if specified and non-empty; otherwise defaults to "tailwind".
     • Starts CLI: tailwind -i <input> -o <output> --watch, logs the command, and keeps the process until app stops.
     • StopAsync kills the process; Dispose disposes it.

2) Integration in apps
   - In Program.cs (server app):
       if (app.Environment.IsDevelopment())
       {
           _ = app.RunTailwind("tailwind", "./"); // or alternative path to client folder
       }
     Add using Tailwind; and configure options appropriately (see README.md for full setup, including package.json script alternative).

3) Sample app specifics (src/SampleApps/BlazorServer)
   - Contains a minimal Tailwind config and Styles/input.css. To see Tailwind in action:
     • Install Node.js and npm.
     • In the BlazorServer folder, ensure tailwindcss is installed in the project (npm install -D tailwindcss cross-env) and tailwind.config.js content globs include .razor/.cshtml.
     • Run dotnet watch run for ASP.NET host; the hosted service will launch the Tailwind CLI in watch mode (Development only), provided Tailwind CLI is available in PATH or TailwindCliPath points to it.

4) Node/Tailwind expectations
   - The service runs a CLI executable named "tailwind" (or TailwindCliPath). If you only have npx, either:
     • Install tailwindcss locally (node_modules/.bin/tailwind) and set TailwindCliPath accordingly (e.g., .\node_modules\.bin\tailwind.cmd on Windows), or
     • Install the tailwindcss standalone CLI and add it to PATH.

5) Logging and diagnostics
   - TailwindHostedService logs the exact command it will run. If CSS isn’t being rebuilt:
     • Confirm app.Environment is Development.
     • Validate TailwindOptions.InputFile/OutputFile paths exist.
     • Ensure Tailwind CLI is resolvable (PATH or explicit TailwindCliPath).
     • Check port/process conflicts are unlikely here, but the repository includes utility classes (TcpPortFinder, EventedStreamReader) used by process plumbing.

6) Code style and conventions
   - C# 10, nullable enabled, implicit usings enabled.
   - Guard pattern for argument validation (see Guard.AgainstNull in src/Tailwind.Extensions.AspNetCore/Guard.cs).
   - Prefer dependency abstractions for external processes (ITailwindProcessInterop) to enable unit testing with fakes.
   - Tests use Arrange/Act/Assert with FakeItEasy for fakes and xUnit for assertions.

7) Common troubleshooting
   - Tests won’t run: Install .NET 6 Runtime (LTS) to match TargetFramework, or multi-target as described above.
   - Tailwind CLI not found: Set TailwindCliPath to the full path to the CLI (Windows often requires the .cmd shim in node_modules/.bin).
   - No CSS updates: Ensure globs in tailwind.config.js include .razor and .cshtml files as shown in README.md.

Quick command reference
- Build: dotnet build -c Release
- Full pipeline: PowerShell ./Build.ps1
- Tests (requires .NET 6 runtime): dotnet test -c Debug
- Pack: dotnet pack .\src\Tailwind.Extensions.AspNetCore -c Release -o .\artifacts --no-build
