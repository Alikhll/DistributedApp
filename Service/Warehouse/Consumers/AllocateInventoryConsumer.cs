using Contract.Warehouse;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Service.Warehouse.Consumers
{
    public class AllocateInventoryConsumer : IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await Task.Delay(500);

            await context.Publish<AllocationCreated>(new AllocationCreated
            {
                AllocationId = context.Message.AllocationId,
                HoldDuration = TimeSpan.FromTicks(8000),
            });

            await context.RespondAsync(new InventoryAllocated
            {
                AllocationId = context.Message.AllocationId,
                ItemNumber = context.Message.ItemNumber,
                Quantity = context.Message.Quantity
            });
        }
    }
}
