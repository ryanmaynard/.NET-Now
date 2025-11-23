using Application.Features.Auth.Commands.Register;
using FluentAssertions;

namespace Application.UnitTests.Features.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterCommand("", "Password123", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterCommand("notanemail", "Password123", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_WhenPasswordIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Pass1", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void Validate_WhenPasswordLacksUppercase_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "password123", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
