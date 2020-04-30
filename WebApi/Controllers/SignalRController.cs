using MassTransit;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignalRController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hub;
        readonly IPublishEndpoint _publishEndpoint;
        public SignalRController(IHubContext<ChatHub> hub,
            IPublishEndpoint publishEndpoint)
        {
            _hub = hub;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Route("Send")]
        public async Task<IActionResult> Send()
        {
            await _hub.Clients.All.SendAsync("ReceiveMessage", "message", "aaa");

            return Accepted();
        }

        [HttpGet]
        [Route("MasstransitSend")]
        public async Task<IActionResult> MasstransitSend()
        {
            var protocols = new IHubProtocol[] { new JsonHubProtocol() };

            await _publishEndpoint.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", "bbb" })
            });

            return Accepted();
        }
    }
}