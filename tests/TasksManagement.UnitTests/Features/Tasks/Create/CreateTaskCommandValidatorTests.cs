using FluentAssertions;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Create;

public class CreateTaskCommandValidatorTests
{
    public static TheoryData<CreateTaskCommand, List<string>> TestData =>
        new()
        {
            {
                new CreateTaskCommand
                {
                    Name = "",
                    Description = "Test description 1",
                    Status = Status.NotStarted,
                    AssignedTo = "user1@example.com"
                },
                new List<string>
                {
                    "Task name is required"
                }
            },
            {
                new CreateTaskCommand
                {
                    Name = "",
                    Description = "",
                    Status = Status.InProgress,
                    AssignedTo = "user2@example.com"
                },
                new List<string>
                {
                    "Task name is required",
                    "Task description is required"
                }
            },
            {
                new CreateTaskCommand
                {
                    Name = "",
                    Description = "",
                    Status = (Status?)5,
                    AssignedTo = "user2@example.com"
                },
                new List<string>
                {
                    "Task name is required",
                    "Task description is required",
                    "Task status is not valid"
                }
            }
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task ValidateAsync_ShouldReturnValidationErrorMessages_WhenCommandHasInvalidPropertyValues(
        CreateTaskCommand command, List<string> expectedValidationErrors)
    {
        // Arrange
        var validator = new CreateTaskCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        var validationErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList();

        result.IsValid.Should().BeFalse();
        validationErrorMessages.Should().BeEquivalentTo(expectedValidationErrors);
    }
}
