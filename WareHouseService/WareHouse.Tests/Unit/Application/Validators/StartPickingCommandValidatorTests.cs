using FluentValidation.TestHelper;
using WareHouse.Application.Commands;
using Xunit;

namespace WareHouse.Tests.Unit.Application.Validators;

public class StartPickingCommandValidatorTests
{
    private readonly StartPickingCommandValidator _validator;

    public StartPickingCommandValidatorTests()
    {
        _validator = new StartPickingCommandValidator();
    }

    [Fact]
    public void Validate_WhenValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.NewGuid(), "picker-123", "Zone-A");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenEmptyOrderId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.Empty, "picker-123", "Zone-A");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void Validate_WhenEmptyPickerId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.NewGuid(), "", "Zone-A");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PickerId);
    }

    [Fact]
    public void Validate_WhenEmptyZone_ShouldHaveValidationError()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.NewGuid(), "picker-123", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Zone);
    }
}