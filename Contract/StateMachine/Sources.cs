﻿using System;

namespace Contract.StateMachine
{
    public class OrderSubmitted
    {
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }

        public string CustomerNumber { get; set; }
    }

    public class CheckOrder
    {
        public Guid OrderId { get; set; }
    }

    public class OrderStatus
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public string CustomerNumber { get; set; }
    }

    public class OrderNotFound
    {
        public Guid OrderId { get; set; }
    }
}
