namespace Tailwind;

public static class Guard
{
    public static void AgainstNull(string? parameterName, string? errorMessage = "")
    {
        if (!string.IsNullOrEmpty(parameterName)) return;
        
        var errorDetails = $"{nameof(parameterName)} must not be null or white space";
        if (!string.IsNullOrEmpty(errorMessage))
            errorDetails += $": {errorMessage}";
            
        throw new ArgumentNullException(errorDetails);
    }
}