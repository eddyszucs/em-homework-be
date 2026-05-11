using ClinicBackend.Validators;
using Xunit;

namespace ClinicBackend.Tests.Unit.Validation;

public class PatientNameValidatorTests
{
    [Theory]
    [InlineData("Kovács Anna", true)]
    [InlineData("Nagy János", true)]
    [InlineData("Tóth Péter", true)]
    [InlineData("Szabó Eszter", true)]
    [InlineData("John Doe", true)]         // ASCII also valid
    [InlineData("Kovács Anna Mária", true)] // three words (edge case — allowed)
    public void ValidNames_ShouldReturnTrue(string name, bool expected)
    {
        var result = PatientNameValidator.IsValid(name);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Kovács")]           // single word
    [InlineData("Anna")]             // single word
    [InlineData("Kovács123")]         // digits
    [InlineData("123 Anna")]          // leading digit
    [InlineData("Anna 456")]          // digit in second name
    [InlineData("")]                  // empty
    [InlineData("   ")]               // whitespace only
    [InlineData(null)]                // null
    public void InvalidNames_ShouldReturnFalse(string? name)
    {
        var result = PatientNameValidator.IsValid(name);
        Assert.False(result);
    }
}