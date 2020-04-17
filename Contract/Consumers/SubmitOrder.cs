using System;

namespace Contract.Consumers
{
    public class SubmitOrder
    {
        public Guid OrderId { get; set; }
        public DateTime Timestmp { get; set; }
        public string OrderCustomer { get; set; }
    }

    public class SubmitOrderAccepted
    {
        public Guid OrderId { get; set; }
        public DateTime Timestmp { get; set; }
        public string OrderCustomer { get; set; }
    }

    public class SubmitOrderRejected
    {
        public Guid OrderId { get; set; }
        public DateTime Timestmp { get; set; }
        public string OrderCustomer { get; set; }
    }
}
