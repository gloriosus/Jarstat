using Jarstat.Application.Commands;
using Jarstat.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jarstat.Presentation.Controllers;

[ApiController]
[Route("api/folders")]
public class FolderController : ControllerBase
{
    private readonly IMediator _mediator;

    public FolderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFolders()
    {
        var query = new GetAllFoldersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFolderById(Guid id)
    {
        var query = new GetFolderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(result);

        return Ok(result);
    }

    // TODO: Consider to implement a method for finding a folder by its VirtualPath

    [HttpPost("create")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderCommand createFolderCommand)
    {
        var result = await _mediator.Send(createFolderCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.ArgumentNullOrWhiteSpaceValue":
                    return BadRequest(result);
                case "Error.ArgumentNullValue":
                    return BadRequest(result);
                case "Error.Exception":
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

        return Created($"api/folders/{result.Value!.Id}", result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateFolder([FromBody] UpdateFolderCommand updateFolderCommand)
    {
        var result = await _mediator.Send(updateFolderCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
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

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        var deleteFolderCommand = new DeleteFolderCommand(id);
        var result = await _mediator.Send(deleteFolderCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.Exception":
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

        return Ok(result);
    }
}
