using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Infrastructure.Attributes;

namespace TelegramPartHook.Controllers
{

    /// <summary>
    /// Don't change this controller name, otherwise you must change it on dialogflow also.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PartituraController : ControllerBase
    {
        private readonly IRequestFactory _requestFactory;
        private readonly IMediator _mediator;
        private readonly ILogHelper _logHelper;
        private readonly ISearchFactory _searchFactory;
        public PartituraController(IRequestFactory requestFactory,
                                   ILogHelper logHelper,
                                   IMediator mediator,
                                   ISearchFactory searchFactory)
        {
            _requestFactory = requestFactory;
            _logHelper = logHelper;
            _mediator = mediator;
            _searchFactory = searchFactory;
        }

        // GET api/partitura/Busca
        [HttpPost("Busca")]
        [BotAuthorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchAsync()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);

            var requestBody = await reader.ReadToEndAsync();

            _logHelper.Info($"CONTENT: {requestBody}", CancellationToken.None);

            var search = await _searchFactory.CreateSearchAsnc(requestBody);

            var mediatorRequest = _requestFactory.DefineRequest(search);

            await _mediator.Send(mediatorRequest);

            return Ok();
        }
    }
}
