using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace TelegramPartHook.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class PrivacidadeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            var privadadeTexto = new StringBuilder("Este aplicativo só faz uso de imagens publicas de partituras postadas no Instagram para fins de pesquisa e indexação.");
            
            return new OkObjectResult(privadadeTexto.ToString());
        }
    }
}
