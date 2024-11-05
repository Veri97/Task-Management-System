using FluentValidation;

namespace TasksManagement.Application.Features.Tasks.Create;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Task name is required")
            .MaximumLength(500)
            .WithMessage("Task name must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Task description is required")
            .MaximumLength(4000)
            .WithMessage("Task description must not exceed 4000 characters");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Task status is not valid");

        RuleFor(x => x.AssignedTo)
            .MaximumLength(500)
            .WithMessage("AssignedTo value must not exceed 500 characters");
    }
}
