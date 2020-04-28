using Contract.Warehouse;
using MassTransit;
using MassTransit.Courier;
using System;
using System.Threading.Tasks;

namespace Service.CourierActivities
{
    public class AllocateInventoryActivity : IActivity<AllocateInventoryArgument, AllocateInventoryLogs>
    {
        private readonly IRequestClient<AllocateInventory> _client;
        public AllocateInventoryActivity(IRequestClient<AllocateInventory> client)
        {
            _client = client;
        }
        public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLogs> context)
        {
            await context.Publish<AllocationReleaseRequested>(new AllocationReleaseRequested
            {
                AllocationId = context.Log.AllocationId,
                Reason = "Order Faulted"
            });

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArgument> context)
        {
            var orderId = context.Arguments.OrderId;
            var itemNumber = context.Arguments.ItemNumber;
            var quantity = context.Arguments.Quantity;

            if (string.IsNullOrEmpty(itemNumber))
                throw new ArgumentNullException(nameof(itemNumber));

            if (quantity <= 0.0m)
                throw new ArgumentNullException(nameof(quantity));

            var allocationId = NewId.NextGuid();

            var response = await _client.GetResponse<InventoryAllocated>(new InventoryAllocated
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });

            return context.Completed(new { AllocationId = allocationId });
        }
    }

    public class AllocateInventoryArgument
    {
        public Guid OrderId { get; set; }
        public string ItemNumber { get; set; }
        public decimal Quantity { get; set; }
    }

    public class AllocateInventoryLogs
    {
        public Guid AllocationId { get; set; }
    }
}
