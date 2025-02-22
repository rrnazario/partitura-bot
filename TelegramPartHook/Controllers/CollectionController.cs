using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelegramPartHook.Application.Queries.Portal;
using TelegramPartHook.Infrastructure.Attributes;

namespace TelegramPartHook.Controllers
{
    [ApiController]
    [PortalAuthorize]
    [Authorize]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CollectionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCollectionAsync()
        {
            var result = await _mediator.Send(new GetAllCollectionListQuery());

            return Ok(result);
        }
    }
}
