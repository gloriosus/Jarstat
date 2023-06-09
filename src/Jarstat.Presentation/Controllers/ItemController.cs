﻿using Jarstat.Application.Commands;
using Jarstat.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderItem([FromBody] ReorderItemCommand reorderItemCommand)
    {
        var result = await _mediator.Send(reorderItemCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.ArgumentLessThanZeroValue":
                    return BadRequest(result);
                case "Error.ArgumentNullOrWhiteSpaceValue":
                    return BadRequest(result);
                case "Error.ArgumentNullValue":
                    return BadRequest(result);
                case "Error.Exception":
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

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
}
