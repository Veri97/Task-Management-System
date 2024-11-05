using FluentAssertions;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Update;

public class UpdateTaskCommandValidatorTests
{
    public static TheoryData<UpdateTaskCommand, List<string>> TestData =>
        new()
        {
            {
                new UpdateTaskCommand
                {
                    Id = -1,
                    NewStatus = Status.NotStarted
                },
                new List<string>
                {
                    "Task Id must be greater than 0"
                }
            },
            {
                new UpdateTaskCommand
                {
                    Id = -2,
                    NewStatus = (Status)5
                },
                new List<string>
                {
                    "Task Id must be greater than 0",
                    "Task status is not valid"
                }
            }
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task ValidateAsync_ShouldReturnValidationErrorMessages_WhenCommandHasInvalidPropertyValues(
        UpdateTaskCommand command, List<string> expectedValidationErrors)
    {
        // Arrange
        var validator = new UpdateTaskCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        var validationErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList();

        result.IsValid.Should().BeFalse();
        validationErrorMessages.Should().BeEquivalentTo(expectedValidationErrors);
    }
}
