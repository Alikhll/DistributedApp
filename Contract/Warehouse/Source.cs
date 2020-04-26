using System;

namespace Contract.Warehouse
{
    public class AllocateInventory
    {
        public Guid AllocationId { get; set; }
        public string ItemNumber { get; set; }
        public decimal Quantity { get; set; }
    }

    public class InventoryAllocated
    {
        public Guid AllocationId { get; set; }
        public string ItemNumber { get; set; }
        public decimal Quantity { get; set; }
    }

    public class AllocationReleaseRequested
    {
        public Guid AllocationId { get; set; }
        public string Reason { get; set; }
    }
}
