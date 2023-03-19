using Jarstat.Application.Commands;
using Jarstat.Application.Queries;
using Jarstat.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jarstat.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand createUserCommand)
    {
        var result = await _mediator.Send(createUserCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.ArgumentNullOrWhiteSpaceValue":
                    return BadRequest(result);
                case "Error.Identity":
                    return BadRequest(result);
            }

        return Created($"api/users/{result.Value!.Id}", result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand updateUserCommand)
    {
        var result = await _mediator.Send(updateUserCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.Identity":
                    return BadRequest(result);
                case "Error.Exception":
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleteUserCommand = new DeleteUserCommand(id);
        var result = await _mediator.Send(deleteUserCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.Identity":
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserCommand loginUserCommand)
    {
        var result = await _mediator.Send(loginUserCommand);

        if (result.IsFailure)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchUsersCommand searchUsersCommand)
    {
        var result = await _mediator.Send(searchUsersCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.ArgumentNotAcceptableValue":
                    return BadRequest(result);
            }

        return Ok(result);
    }
}
