namespace Tailwind;

public static class Guard
{
    public static void AgainstNull(string? parameterName, string? errorMessage = "")
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            throw new ArgumentNullException($"{parameterName} must not be null or white space. ${errorMessage}");
        }
    }
}