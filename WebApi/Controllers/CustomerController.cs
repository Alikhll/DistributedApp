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
    public class CustomerController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CustomerController(
            IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(Guid customerId)
        {

            await _publishEndpoint.Publish<CustomerAccountClosed>(new CustomerAccountClosed
            {
                CustomerId = customerId
            });

            return Accepted();
        }
    }
}