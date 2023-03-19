using Jarstat.Application.Commands;
using Jarstat.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jarstat.Presentation.Controllers;

[ApiController]
[Route("api/items")]
public class ItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public ItemController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllItems()
    {
        var query = new GetAllItemsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("move")]
    public async Task<IActionResult> ChangeItemPosition([FromBody] ChangeItemPositionCommand changeItemPositionCommand)
    {
        var result = await _mediator.Send(changeItemPositionCommand);

        return Ok(result);
    }

    [HttpGet("roots")]
    public async Task<IActionResult> GetRoots()
    {
        var query = new GetRootsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("children/{parentId}")]
    public async Task<IActionResult> GetChildren(Guid parentId)
    {
        var query = new GetChildrenQuery(parentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchDocuments([FromBody] SearchDocumentsCommand searchDocumentsCommand)
    {
        var result = await _mediator.Send(searchDocumentsCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.ArgumentNotAcceptableValue":
                    return BadRequest(result);
            }

        return Ok(result);
    }
}
