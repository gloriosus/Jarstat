using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Jarstat.Application.Commands;
using Jarstat.Application.Queries;

namespace Jarstat.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDocuments()
    {
        var query = new GetAllDocumentsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocumentById(Guid id)
    {
        var query = new GetDocumentByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentCommand createDocumentCommand)
    {
        var result = await _mediator.Send(createDocumentCommand);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.EntryNotFound":
                    return BadRequest(result);
                case "Error.Exception":
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

        return Created($"api/documents/{result.Value!.Id}", result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateDocument([FromBody] UpdateDocumentCommand updateDocumentCommand)
    {
        var result = await _mediator.Send(updateDocumentCommand);

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

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var deleteDocumentCommand = new DeleteDocumentCommand(id);
        var result = await _mediator.Send(deleteDocumentCommand);

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

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var query = new DownloadFileQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            switch (result.Error.Code)
            {
                case "Error.ArgumentNullValue":
                    return NotFound(result);
                case "Error.EntryNotFound":
                    return NotFound(result);
            }

        var fileValue = result.Value!.File!.Value;
        var fileName = result.Value!.FileName;

        return File(fileValue, "application/octet-stream", fileName);
    }

    // The 1st step: upload a file to the folder specified in configuration on the server
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileCommand uploadFileCommand)
    {
        var result = await _mediator.Send(uploadFileCommand);
        return Ok(result);
    }

    // The 2nd step: copy the file saved in the folder to the database
    [HttpPost("copy")]
    public async Task<IActionResult> CopyFile([FromBody] CopyFileCommand copyFileCommand)
    {
        var result = await _mediator.Send(copyFileCommand);

        if (result is null)
            return NotFound(result);

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
