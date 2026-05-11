using System.Text.RegularExpressions;

namespace ClinicBackend.Validators;

public static partial class PatientNameValidator
{
    private static readonly Regex NameRegex = new(@"^[^\d\s]+ [^\d\s]+", RegexOptions.Compiled);

    public static bool IsValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return NameRegex.IsMatch(name);
    }
}