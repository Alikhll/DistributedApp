using Contract.Consumers;
using Contract.StateMachine;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IRequestClient<SubmitOrder> _submitOrderRequestClient;
        private readonly IRequestClient<CheckOrder> _checkOrderRequestClient;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public HomeController(IRequestClient<SubmitOrder> submitOrderRequestClient,
            IRequestClient<CheckOrder> checkOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider)
        {
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _checkOrderRequestClient = checkOrderRequestClient;
        }

        [HttpGet]
        [Route("Request")]
        public async Task<IActionResult> RequestSend()
        {
            var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<SubmitOrderAccepted, SubmitOrderRejected>(new SubmitOrder { OrderCustomer = "" });

            if (accepted.IsCompletedSuccessfully)
                return Ok((await accepted).Message);

            return BadRequest((await rejected).Message);
        }

        [HttpGet]
        [Route("Send")]
        public async Task<IActionResult> Send()
        {

            //string consumer = KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>();
            //respond = submit-order

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));
            await endpoint.Send<SubmitOrder>(new SubmitOrder { OrderCustomer = "A" });

            return Ok();
        }

        [HttpGet]
        [Route("OrderStatus")]
        public async Task<IActionResult> OrderStatus()
        {
            var guid = Guid.Parse("EC8C32B3-E71C-4072-B6B1-0BEA6A342695");
            //var guid = Guid.Parse("EC8C32B3-E71C-4072-B6B1-0BEA6A342691");

            var (status, notFound) = await _checkOrderRequestClient.GetResponse<OrderStatus, OrderNotFound>(new CheckOrder
            {
                OrderId = guid
            });

            if (status.IsCompletedSuccessfully)
            {
                return Ok((await status).Message);
            }

            return NotFound((await notFound).Message);
        }
    }
}