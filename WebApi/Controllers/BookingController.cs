using Contract.Booking;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        readonly IPublishEndpoint _publishEndpoint;
        public BookingController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        [Route("Book")]
        public async Task<IActionResult> Book(BookingAccepted model)
        {
            model.BookingId = NewId.NextGuid();
            await _publishEndpoint.Publish(model);

            return Accepted();
        }
    }
}