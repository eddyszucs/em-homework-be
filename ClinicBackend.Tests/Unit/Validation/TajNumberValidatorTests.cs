using ClinicBackend.Validators;
using Xunit;

namespace ClinicBackend.Tests.Unit.Validation;

public class TajNumberValidatorTests
{
    [Theory]
    [InlineData("123-456-789", true)]
    [InlineData("000-000-000", true)]
    [InlineData("999-999-999", true)]
    public void ValidTajNumbers_ShouldReturnTrue(string taj, bool expected)
    {
        var result = TajNumberValidator.IsValid(taj);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123-45-789")]      // only 8 digits
    [InlineData("1234-56-789")]     // too long
    [InlineData("123-456-7890")]    // extra digit
    [InlineData("12-345-678")]      // wrong dash positions
    [InlineData("abc-def-ghi")]     // letters
    [InlineData("123 456 789")]     // spaces instead of dashes
    [InlineData("123456789")]       // no dashes
    [InlineData("")]                // empty
    [InlineData(null)]              // null
    [InlineData("   ")]             // whitespace
    public void InvalidTajNumbers_ShouldReturnFalse(string? taj)
    {
        var result = TajNumberValidator.IsValid(taj);
        Assert.False(result);
    }
}