using System;
using System.Collections.Generic;

namespace NovaCommerce.Inventory.Application.Dtos
{
    public class OrderCancelledEvent
    {
        public Guid OrderId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
