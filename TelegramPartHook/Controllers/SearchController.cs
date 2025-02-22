using Light.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TelegramPartHook.Application.Queries;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Infrastructure.Attributes;

namespace TelegramPartHook.Controllers;

[ApiController]
[PortalAuthorize]
[Authorize]    
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPdfService _pdfService;

    public SearchController(IMediator mediator, 
                            IPdfService pdfService)
    {
        _mediator = mediator.MustNotBeNull();
        _pdfService = pdfService.MustNotBeNull();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAsync([FromQuery]string term)
    {
        var result = await _mediator.Send(new GetSheetLinksQuery(term));

        return Ok(result);
    }

    [HttpGet("pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePdfAsync([FromQuery] string term)
    {
        var result = await _mediator.Send(new GetSheetLinksQuery(term));

        var pdfPath = await _pdfService.GenerateAsync([..result], term);

        return Ok(Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(pdfPath)));
    }
}
