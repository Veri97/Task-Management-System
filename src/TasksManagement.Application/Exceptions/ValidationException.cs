namespace TasksManagement.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(List<string> errors)
    {
        Errors = errors;
    }

    public List<string> Errors { get; }
}
