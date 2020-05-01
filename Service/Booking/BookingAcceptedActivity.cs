using Automatonymous;
using Contract.Booking;
using GreenPipes;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Service.Booking
{
    public class BookingAcceptedActivity : Activity<BookingState, BookingAccepted>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<BookingState, BookingAccepted> context, Behavior<BookingState, BookingAccepted> next)
        {
            var consumeContext = context.GetPayload<ConsumeContext>();

            var sendEndpoint = await consumeContext.GetSendEndpoint(new Uri("queue:booking"));

            await sendEndpoint.Send(new BookingModel
            {
                BookingId = context.Data.BookingId,
                Car = context.Data.Car,
                Flight = context.Data.Flight,
                Hotel = context.Data.Hotel,
            });

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<BookingState, BookingAccepted, TException> context, Behavior<BookingState, BookingAccepted> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("accept-booking");
        }
    }
}
