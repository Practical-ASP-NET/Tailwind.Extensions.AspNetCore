_Disclaimer: This project is not affiliated with or supported by Tailwind Labs._

# Use Tailwind's JIT mode with `dotnet run` and `dotnet watch run`

Makes it possible to use the new "Just In Time" builds for Tailwind (Tailwind 3+) with ASP.NET Core.

Works for Blazor WASM applications that are hosted via ASP.NET Core, and Blazor Server applications.

Note it doesn't work with Blazor WASM apps that aren't hosted via ASP.NET Core.

## Usage

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
@tailwind base;
@tailwind components;
@tailwind utilities;
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
dotnet add package Tailwind.Extensions.AspNetCore --version 1.0.0-beta2
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
