using System.Text.RegularExpressions;

namespace ClinicBackend.Validators;

public static partial class TajNumberValidator
{
    private static readonly Regex TajRegex = new(@"^\d{3}-\d{3}-\d{3}$", RegexOptions.Compiled);

    public static bool IsValid(string? taj)
    {
        if (string.IsNullOrWhiteSpace(taj)) return false;
        return TajRegex.IsMatch(taj);
    }
}