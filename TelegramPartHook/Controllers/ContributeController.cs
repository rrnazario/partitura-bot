using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelegramPartHook.Application.Commands.Contribution;

namespace TelegramPartHook.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContributeController
    : ControllerBase
{
    private readonly IMediator _mediator;

    public ContributeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Post(CancellationToken cancellationToken)
    {
        var command = new ContributeCommand(Request.Form);

        await _mediator.Send(command, cancellationToken);

        return Ok();
    }
}