using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebApi.SignalR;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignalRController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hub;
        public SignalRController(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        [HttpGet]
        [Route("Send")]
        public async Task<IActionResult> Send()
        {
            await _hub.Clients.All.SendAsync("ReceiveMessage", "message", "aaaa");

            return Accepted();
        }
    }
}