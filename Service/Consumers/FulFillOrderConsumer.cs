using Contract.Consumers;
using Contract.Warehouse;
using MassTransit;
using MassTransit.Courier;
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
                new AllocateInventory
                {
                    ItemNumber = "Item123",
                    Quantity = 10.0m
                });

            builder.AddVariable("OrderId", context.Message.OrderId);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
