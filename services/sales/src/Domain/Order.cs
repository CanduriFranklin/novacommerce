using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Sales.Domain
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public Guid CustomerId { get; set; }

        public OrderStatus Status { get; set; }

        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Rejected
    }
}
