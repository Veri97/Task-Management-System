namespace TasksManagement.Api.Errors;

public class ApiErrorResponse
{
    public ApiErrorResponse(int statusCode, List<string> errors)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public int StatusCode { get; set; }
    public List<string> Errors { get; set; }
}
