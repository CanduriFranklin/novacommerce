using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Domain
{
    public class ProcessedEvent
    {
        public Guid OrderId { get; set; }
        public string EventType { get; set; } // e.g., "OrderConfirmed", "OrderCancelled"
        public DateTime ProcessedAt { get; set; }
    }
}
