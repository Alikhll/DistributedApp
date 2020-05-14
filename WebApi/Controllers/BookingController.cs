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
        private readonly IRequestClient<BookingRequestResponseModel> _bookingModelRequestClient;

        public BookingController(IPublishEndpoint publishEndpoint,
            IRequestClient<BookingRequestResponseModel> bookingModelRequestClient)
        {
            _publishEndpoint = publishEndpoint;
            _bookingModelRequestClient = bookingModelRequestClient;
        }

        [HttpPost]
        [Route("BookingWithSaga")]
        public async Task<IActionResult> BookingWithSaga(BookingAccepted model)
        {
            model.BookingId = NewId.NextGuid();
            await _publishEndpoint.Publish(model);

            return Accepted();
        }

        [HttpPost]
        [Route("BookingWithoutSaga")]
        public async Task<IActionResult> BookingWithoutSaga(BookingModel model)
        {
            model.BookingId = NewId.NextGuid();
            await _publishEndpoint.Publish(model);

            return Accepted();
            
            // It does not send finalized because of 'contex.SourceAddress' in BookingConsumer
            //await builder.AddSubscription(context.SourceAddress,
            //    RoutingSlipEvents.Completed | RoutingSlipEvents.Supplemental,
            //    RoutingSlipEventContents.None, x => x.Send(new BookingFinalized { BookingId = context.Message.BookingId }));
        }

        [HttpPost]
        [Route("BookingRequestResponse")]
        public async Task<IActionResult> BookingRequestResponse(BookingRequestResponseModel model)
        {
            model.BookingId = NewId.NextGuid();
            var response = await _bookingModelRequestClient.GetResponse<BookingRequestResponseModel>(model);

            return Accepted(response.Message.BookingId);
        }
    }
}