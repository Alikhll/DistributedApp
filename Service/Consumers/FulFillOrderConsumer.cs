using Contract.Consumers;
using Contract.StateMachine;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using Service.CourierActivities;
using System;
using System.Threading.Tasks;

namespace Service.Consumers
{
    public class FulfillOrderConsumer : IConsumer<FulfillOrder>
    {
        public async Task Consume(ConsumeContext<FulfillOrder> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            //AllocateInventoryActivity
            builder.AddActivity("AllocateInventory", new Uri("queue:allocate-inventory_execute"),
                new AllocateInventoryArgument
                {
                    ItemNumber = "Item123",
                    Quantity = 10.0m
                });

            builder.AddActivity("PaymentActivity", new Uri("queue:payment_execute"),
               new PaymentArgument
               {
                   CardNumber = "4444",
                   Amount = 10
               });

            builder.AddVariable("OrderId", context.Message.OrderId);

            await builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.Faulted,
                RoutingSlipEventContents.None, x => x.Send<OrderFulfilmentFaulted>(
                new OrderFulfilmentFaulted
                {
                    OrderId = context.Message.OrderId,
                }));

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
