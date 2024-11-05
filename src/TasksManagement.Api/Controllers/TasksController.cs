using MediatR;
using Microsoft.AspNetCore.Mvc;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Application.Features.Tasks.GetAll;
using TasksManagement.Application.Features.Tasks.Update;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTasksQuery(), cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateTaskCommand { Id = id, NewStatus = request.NewStatus }, cancellationToken);

        return NoContent();
    }
}
