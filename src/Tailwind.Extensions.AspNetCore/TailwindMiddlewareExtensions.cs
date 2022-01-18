using Microsoft.AspNetCore.Builder;

namespace Tailwind;

/// <summary>
/// Extension methods for enabling React development server middleware support.
/// </summary>
public static class TailwindMiddlewareExtensions
{
    /// <summary>
    /// Automatically runs an npm script
    /// This feature should probably only be used in development.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="npmScript">The name of the script in your package.json file that launches Tailwind.</param>
    /// <param name="workingDir">The directory where your package.json file resides</param>
    public static Task RunTailwind(
        this IApplicationBuilder applicationBuilder,
        string npmScript, string workingDir = "./")
    {
        if (applicationBuilder == null)
        {
            throw new ArgumentNullException(nameof(applicationBuilder));
        }

        return TailwindMiddleware.Attach(applicationBuilder, npmScript, workingDir);
    }
}
