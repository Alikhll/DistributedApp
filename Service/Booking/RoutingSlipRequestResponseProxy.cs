using Contract.Booking;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using System;
using System.Threading.Tasks;

namespace Service.Booking
{
    //For some reason I do not know yet request and response were in different endpoint
    public class RequestProxylDefinition : ConsumerDefinition<RequestProxy>
    {
        public RequestProxylDefinition()
        {
            EndpointName = "booking_proxy_queue";
        }
    }
    public class RequestProxy :
        RoutingSlipRequestProxy<BookingRequestResponseModel>
    {
        protected override Task BuildRoutingSlip(RoutingSlipBuilder builder, ConsumeContext<BookingRequestResponseModel> request)
        {
            builder.AddActivity("BookFlight", new Uri("queue:book-flight_execute"),
                new BookFlightArgument
                {
                    Message = request.Message.Flight
                });

            builder.AddActivity("BookHotel", new Uri("queue:book-hotel_execute"),
                new BookHotelArgument
                {
                    Message = request.Message.Hotel
                });

            builder.AddActivity("BookCar", new Uri("queue:book-car_execute"),
                new BookCarArgument
                {
                    Message = request.Message.Car
                });

            return Task.FromResult(0);
        }
    }

    //For some reason I do not know yet request and response were in different endpoint
    public class ResponseProxyDefinition : ConsumerDefinition<ResponseProxy>
    {
        public ResponseProxyDefinition()
        {
            EndpointName = "booking_proxy_queue";
        }
    }
    public class ResponseProxy :
        RoutingSlipResponseProxy<BookingRequestResponseModel, BookingRequestResponseModel>
    {
        protected override Task<BookingRequestResponseModel> CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, BookingRequestResponseModel request)
        {
            return Task.FromResult(new BookingRequestResponseModel
            {
                BookingId = request.BookingId,
                Car = "WOW"
            });
        }

        protected override Task<Fault<BookingRequestResponseModel>> CreateFaultedResponseMessage(ConsumeContext<RoutingSlipFaulted> context, BookingRequestResponseModel request, Guid requestId)
        {
            return base.CreateFaultedResponseMessage(context, request, requestId);
        }
    }
}
