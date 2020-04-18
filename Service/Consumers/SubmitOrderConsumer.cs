using Contract.Consumers;
using Contract.StateMachine;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using System.Threading.Tasks;

namespace Service.Consumers
{
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            await Task.Delay(10);

            if (context.RequestId != null) //is request/response
            {
                if (context.Message.OrderCustomer.Contains("test"))
                {
                    await context.RespondAsync<SubmitOrderRejected>(new SubmitOrderRejected
                    {
                        OrderId = context.Message.OrderId,
                        OrderCustomer = context.Message.OrderCustomer,
                        Timestmp = InVar.Timestamp,
                    });
                    return;
                }
                else
                {
                    await context.RespondAsync<SubmitOrderAccepted>(new SubmitOrderAccepted
                    {
                        OrderId = context.Message.OrderId,
                        OrderCustomer = context.Message.OrderCustomer,
                        Timestmp = InVar.Timestamp,
                    });
                }
            }

            //Saga
            await context.Publish<OrderSubmitted>(new OrderSubmitted
            {
                OrderId = context.Message.OrderId,
                CustomerNumber = context.Message.OrderCustomer,
                Timestamp = InVar.Timestamp
            });
        }
    }
}
