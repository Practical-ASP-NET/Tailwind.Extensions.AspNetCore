_Disclaimer: This project is not affiliated with or supported by Tailwind Labs._

# Use Tailwind's JIT mode with `dotnet run` and `dotnet watch run`

Makes it possible to use "Just In Time" builds for Tailwind (Tailwind 3+) with ASP.NET Core.

Works for Blazor WASM applications that are hosted via ASP.NET Core, and Blazor Server applications.

Note this doesn't work with Blazor WASM apps that aren't hosted via ASP.NET Core.

## Recommended: Use the Tailwind CLI (Hosted Service)

The simplest way to integrate Tailwind during Development is to use the Tailwind CLI directly via this library's hosted service.

Simple path (recommended):
- Download the official Tailwind CSS standalone CLI for your OS and add it to your system PATH so the command `tailwind` is available, OR
- Download it and set the full path in configuration via `Tailwind:TailwindCliPath`.

There is no auto-download of the Tailwind CLI. The CLI must be present on PATH or referenced by a full path.

1) Install the NuGet package in your ASP.NET Core app:

```powershell
 dotnet add package Tailwind.Extensions.AspNetCore
```

2) Configure Program.cs before `Build()`:

```csharp
using Tailwind;

var builder = WebApplication.CreateBuilder(args);

// other services...

// Enable Tailwind CLI watcher (Development only)
builder.UseTailwindCli();

var app = builder.Build();
```

3) Add configuration (appsettings.Development.json):

```json
{
  "Tailwind": {
    "InputFile": "./Styles/input.css",
    "OutputFile": "./wwwroot/css/output.css",
    "TailwindCliPath": "" // optional: set full path if not on PATH
  }
}
```

Notes:
- If `TailwindCliPath` is empty, the service runs a command named `tailwind` from PATH.
- Secondary option (if you use npm-installed CLI): on Windows you can point to the npm shim, e.g. `.\\node_modules\\.bin\\tailwind.cmd`.
- The hosted service is a no-op outside Development and stops the CLI when the app stops.

4) Ensure Tailwind CLI is available:
- Preferred: use the standalone Tailwind CLI (downloaded binary) and add it to PATH or set `TailwindCliPath`.
- Alternative: install via npm in your project (`npm install -D tailwindcss`) and ensure the CLI is resolvable (PATH or `TailwindCliPath`).

5) Run your app:
Run `dotnet watch run`. In Development, the service will run `tailwind -i <input> -o <output> --watch` and log the exact command.

### Sample
See `src/SampleApps/BlazorServer` for a working setup (Program.cs calls `builder.UseTailwindCli();`, appsettings.Development.json contains the Tailwind section).

---

## Alternative: Use an npm script

If you prefer to keep Tailwind as an npm script, you can use this extension to run your npm script during Development.

Create a new Hosted Blazor WASM, or Blazor Server project.

CD to the Client App's folder (if Blazor WASM) or Blazor Server App's folder.

Run these commands:
``` powershell
npm install -D tailwindcss cross-env
npx tailwindcss init
```

Create a new Hosted Blazor WASM, or Blazor Server project.

CD to the Client App's folder (if Blazor WASM) or Blazor Server App's folder.

Run these commands:
``` powershell
npm install -D tailwindcss cross-env
npx tailwindcss init
```

This will install Tailwind and the handy cross-env utility via NPM, then create a `tailwind.config.js` file.

Now update the `tailwind.config.js` file to include all your .razor and .cshtml files.

``` javascript
module.exports = {
  content: ["**/*.razor", "**/*.cshtml", "**/*.html"],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

Now you'll want to create the Tailwind input stylesheet. This is the stylesheet that Tailwind will then pick up and build.

Here's the minimum you'll need...

**Styles\input.css**

``` css
@import "tailwindcss";
```

Finally, update your `package.json` file to add this script.

``` json
  "scripts": {
    "tailwind": "cross-env NODE_ENV=development ./node_modules/tailwindcss/lib/cli.js -i ./Styles/input.css -o ./wwwroot/css/output.css --watch"
  },
```

Make sure **.Styles/input.css** is pointing to the css file you created in the last step. You can control where the resulting css file is created by specifying your own value for the `-o` parameter.

That takes care of the Tailwind setup, now we just need to make this run when you run your ASP.NET Core project during Development.

Make sure you switch to the Server App's folder if you're using Blazor WASM (hosted).

Run this command to install the Tailwind AspNetCore NuGet package.

``` powershell
dotnet add package Tailwind.Extensions.AspNetCore
```

Now head over to `Program.cs` and add this code before `app.Run()`;

``` csharp
if (app.Environment.IsDevelopment())
{
    _ = app.RunTailwind("tailwind", "./");
}
```

The second argument is the path to the folder containing your package.json file. If you're using Blazor WASM you'll probably need something like this...

``` csharp
if (app.Environment.IsDevelopment())
{
    _ = app.RunTailwind("tailwind", "../Client/");
}
```

You'll also need to add this `using` statement:

``` csharp
using Tailwind;
```

Note we're using the discard parameter `_` when we call `app.RunTailwind`. This is because the method is async, but we don't want to wait for it to complete (as this would cause your app to wait for it to finish, and we want it continue running in the background alongside your app). Using the `_` parameter here stops your IDE nagging at you to await the call 😀 

Now, run `dotnet watch run` and try modifying your Razor components (using Tailwind's utility classes).

You should see logs indicating that tailwind has rebuilt the CSS stylesheet successfully.

---

## Choosing an approach

You can use either approach depending on your preference:

- Tailwind CLI hosted service (recommended): configure via appsettings and call `builder.UseTailwindCli();`. No npm script required. Prefer this with the standalone Tailwind CLI on PATH or set via `TailwindCliPath`.
- npm script middleware (alternative): keep your Tailwind build as an npm script and call `_ = app.RunTailwind("tailwind", "./");` in Development. This remains fully supported.
