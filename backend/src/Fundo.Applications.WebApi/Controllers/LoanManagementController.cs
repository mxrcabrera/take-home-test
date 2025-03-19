using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [Route("/loan")]
    public class LoanManagementController : Controller
    {
        [HttpGet]
        public Task<ActionResult> Get() {
            return Task.FromResult<ActionResult>(Ok());
        }
    }
}