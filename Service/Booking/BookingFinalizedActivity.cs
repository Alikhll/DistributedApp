using Automatonymous;
using Contract.Booking;
using GreenPipes;
using MassTransit;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace Service.Booking
{
    public class BookingFinalizedActivity : Activity<BookingState, BookingFinalized>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<BookingState, BookingFinalized> context, Behavior<BookingState, BookingFinalized> next)
        {
            await Task.Delay(1000);

            var consumeContext = context.GetPayload<ConsumeContext>();

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await consumeContext.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "done", $"Booking was successfully finalized!" })
            });

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<BookingState, BookingFinalized, TException> context, Behavior<BookingState, BookingFinalized> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("finalized-booking");
        }
    }
}
