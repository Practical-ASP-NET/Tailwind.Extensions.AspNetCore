# Findings: Console hotkeys blocked by Tailwind CLI background process

Date: 2025-09-14

## Summary
When running an ASP.NET Core app with `dotnet watch`, console hotkeys such as `Ctrl+R` (restart) were not working after enabling the Tailwind hosted service. The Tailwind process was started in the background, but it inherited the parent console's standard input (stdin). As a result, keyboard input could be observed by the child process and not by `dotnet watch`, effectively blocking or interfering with hotkeys.

## Root cause
- The hosted service launches Tailwind via `TailwindProcessInterop.StartProcess`.
- That method configured `ProcessStartInfo` to redirect standard output and error, but not standard input.
- With `UseShellExecute = false` and without `RedirectStandardInput = true`, the child process inherits the console stdin handle from the parent process. Some CLIs (including Node/Tailwind) can attach to or read from stdin in watch mode, which prevents `dotnet watch` from receiving hotkey keystrokes.

## Fix
- Set `RedirectStandardInput = true` when starting the Tailwind process. This connects the child process stdin to an anonymous pipe instead of the console, ensuring that the parent process (`dotnet watch`) remains the sole consumer of console keyboard input.

Code change (TailwindProcessInterop.cs):
- Added `RedirectStandardInput = true` to `ProcessStartInfo` alongside the existing stdout/stderr redirection.

This preserves the existing behavior (background process, output piped back to the app console) while ensuring keyboard hotkeys keep working for the main .NET process.

## Tests
- Added `TailwindProcessInteropTests.StartProcess_ShouldRedirectStandardInput_ToAvoidConsoleHotkeyInterference` to verify the interop starts a process with `RedirectStandardInput` enabled. This is a minimal, platform-aware test that executes a short-lived command and asserts the configuration is correct.
- Existing tests for `TailwindHostedService` remain intact and continue to validate option handling and CLI path selection.

Note: Running tests requires the .NET 6 runtime (TargetFramework=net6.0). See repository guidelines for installation or temporary multi-targeting if needed.

## How to verify manually
1. Ensure Tailwind CLI is installed and resolvable (either via PATH or `TailwindCliPath`).
2. Run your ASP.NET Core app using `dotnet watch run` with the Tailwind hosted service enabled.
3. Confirm that Tailwind output appears in the console as before.
4. Press `Ctrl+R` or other `dotnet watch` hotkeys.
   - Expected: hotkeys are recognized by `dotnet watch` (e.g., app restarts) while Tailwind continues to run in the background.

## Considerations
- We kept `UseShellExecute = false` to allow stream redirection and `CreateNoWindow = true` so the Tailwind process does not spawn a separate console window.
- No breaking changes to the public API were introduced. The fix is internal to process start configuration.
