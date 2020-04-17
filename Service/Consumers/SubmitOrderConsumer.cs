using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using Contract.StateMachine;
using System;
using System.Threading.Tasks;
using Contract.Consumers;

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
                        OrderCustomer = "rejected",
                        Timestmp = InVar.Timestamp
                    });
                }
                else
                {
                    //Saga
                    var guid = Guid.Parse("EC8C32B3-E71C-4072-B6B1-0BEA6A342695");
                    await context.Publish<OrderSubmitted>(new OrderSubmitted {
                        OrderId = guid,
                        CustomerNumber = "cusNumber"
                    });

                    await context.RespondAsync<SubmitOrderAccepted>(new SubmitOrderAccepted
                    {
                        OrderCustomer = "accepted",
                        Timestmp = InVar.Timestamp
                    });
                }
            }
        }
    }
}
