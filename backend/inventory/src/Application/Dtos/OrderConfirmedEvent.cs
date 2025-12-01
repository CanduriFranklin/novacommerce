using System;
using System.Collections.Generic;

namespace NovaCommerce.Inventory.Application.Dtos
{
    public class OrderConfirmedEvent
    {
        public Guid OrderId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
