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
    public class OrderController : ControllerBase
    {
        private readonly IRequestClient<SubmitOrder> _submitOrderRequestClient;
        private readonly IRequestClient<CheckOrder> _checkOrderRequestClient;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderController(IRequestClient<SubmitOrder> submitOrderRequestClient,
            IRequestClient<CheckOrder> checkOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider)
        {
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _checkOrderRequestClient = checkOrderRequestClient;
        }

        //create saga
        [HttpGet]
        [Route("Request")]
        public async Task<IActionResult> RequestSend(Guid orderId, string customerName)
        {
            var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<SubmitOrderAccepted, SubmitOrderRejected>(new SubmitOrder
            {
                OrderCustomer = customerName,
                OrderId = orderId
            });

            if (accepted.IsCompletedSuccessfully)
                return Ok((await accepted).Message);

            return BadRequest((await rejected).Message);
        }

        //update saga
        [HttpGet]
        [Route("Send")]
        public async Task<IActionResult> Send(Guid orderId, string customerName)
        {

            //string consumer = KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>();
            //respond = submit-order

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));
            await endpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderCustomer = customerName,
                OrderId = orderId
            });

            return Ok();
        }

        [HttpGet]
        [Route("OrderStatus")]
        public async Task<IActionResult> OrderStatus(Guid orderId)
        {

            var (status, notFound) = await _checkOrderRequestClient.GetResponse<OrderStatus, OrderNotFound>(new CheckOrder
            {
                OrderId = orderId
            });

            if (status.IsCompletedSuccessfully)
            {
                return Ok((await status).Message);
            }

            return NotFound((await notFound).Message);
        }
    }
}