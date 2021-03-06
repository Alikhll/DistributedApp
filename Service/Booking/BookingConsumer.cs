﻿using Contract.Booking;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using System;
using System.Threading.Tasks;

namespace Service.Booking
{
    public class BookingConsumerDefinition :
        ConsumerDefinition<BookingConsumer>
    {
        public BookingConsumerDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<BookingConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r =>
            {
                r.Interval(3, 1000);
            });
        }
    }

    public class BookingConsumer : IConsumer<BookingModel>
    {
        public async Task Consume(ConsumeContext<BookingModel> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddActivity("BookFlight", new Uri("queue:book-flight_execute"),
                new BookFlightArgument
                {
                    Message = context.Message.Flight
                });

            builder.AddActivity("BookHotel", new Uri("queue:book-hotel_execute"),
                new BookHotelArgument
                {
                    Message = context.Message.Hotel
                });

            //Retry activity has applied in its file
            builder.AddActivity("BookCar", new Uri("queue:book-car_execute"),
                new BookCarArgument
                {
                    Message = context.Message.Car
                });

            await builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Faulted | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None, x => x.Send(new BookingFaulted { BookingId = context.Message.BookingId }));

            await builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Completed | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None, x => x.Send(new BookingFinalized { BookingId = context.Message.BookingId }));


            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }

    public static class BookItem
    {
        public const int LongDelay = 500;
    }
}
