using System;
using System.Collections.Generic;

namespace NovaCommerce.Inventory.Application
{
    public class OrderCancelledEvent
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    // This OrderItem DTO should match the one used in the Sales service's events
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
