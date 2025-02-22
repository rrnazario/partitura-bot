using Light.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TelegramPartHook.Application.Commands.Repertoire;
using TelegramPartHook.Application.Extensions;
using TelegramPartHook.Application.Queries.Portal;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Attributes;

namespace TelegramPartHook.Controllers
{
    [ApiController]
    [PortalAuthorize, Authorize]
    [Route("api/[controller]")]
    public class RepertoireController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RepertoireController(IMediator mediator)
        {
            _mediator = mediator.MustNotBeNull();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync()
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new GetRepertoireQuery(portalUser);

            return Ok(await _mediator.Send(request));
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddAsync([FromBody] SheetSearchResult sheet)
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new AddRepertoireCommand(portalUser, sheet);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPatch("up")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpAsync([FromBody] SheetSearchResult sheet)
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new HandleRepertoireOrderCommand(portalUser, sheet, RepertoireOrder.Up);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPatch("down")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DownAsync([FromBody] SheetSearchResult sheet)
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new HandleRepertoireOrderCommand(portalUser, sheet, RepertoireOrder.Down);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPatch("remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAsync([FromBody] SheetSearchResult sheet)
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new RemoveRepertoireCommand(portalUser, sheet);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPatch("first")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> FistAsync([FromBody] SheetSearchResult sheet)
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new HandleRepertoireOrderCommand(portalUser, sheet, RepertoireOrder.First);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPatch("last")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LastAsync([FromBody] SheetSearchResult sheet)
        {
            var portalUser = HttpContext.ExtractPortalUser();
            var request = new HandleRepertoireOrderCommand(portalUser, sheet, RepertoireOrder.Last);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearAsync()
        {
            var portalUser = HttpContext.ExtractPortalUser();
            
            var request = new HandleRepertoireOrderCommand(portalUser, new("", Domain.Constants.Enums.FileSource.Generated), RepertoireOrder.Clear);

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPost("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadAsync()
        {
            var portalUser = HttpContext.ExtractPortalUser();

            var request = new GeneratePDFRepertoireCommand(portalUser);

            var path = await _mediator.Send(request);

            return Ok(Convert.ToBase64String(System.IO.File.ReadAllBytes(path)));
        }
    }
}
