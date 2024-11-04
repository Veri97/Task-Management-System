using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TasksManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
