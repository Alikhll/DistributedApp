using Contract.Consumers;
using Contract.StateMachine;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace Service.Consumers
{
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            //endpointConfigurator.DiscardFaultedMessages();
            //endpointConfigurator.DiscardSkippedMessages();

            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

    /*
     * By default, MassTransit will move faulted messages to the _error queue. This behavior can be customized for each receive endpoint.
     * Just be called when we have fan in/out not request/response
     */
    public class SubmitOrderFaultConsumer : IConsumer<Fault<SubmitOrder>>
    {
        public Task Consume(ConsumeContext<Fault<SubmitOrder>> context)
        {
            return Task.FromResult(0);
        }
    }

    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", "ccc" })
            });

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
