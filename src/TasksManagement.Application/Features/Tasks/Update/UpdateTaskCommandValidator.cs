using FluentValidation;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
           .GreaterThan(default(int))
           .WithMessage($"Task Id must be greater than {default(int)}");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Task status is not valid");
    }
}
