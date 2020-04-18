using Automatonymous;
using Contract.StateMachine;
using GreenPipes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service.StateMachine
{
    class AcceptOrderActivity : Activity<OrderState, OrderAccepted>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, OrderAccepted> context, Behavior<OrderState, OrderAccepted> next)
        {
            Console.WriteLine(context.Data.OrderId);

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAccepted, TException> context, Behavior<OrderState, OrderAccepted> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("accept-order");
        }
    }
}
