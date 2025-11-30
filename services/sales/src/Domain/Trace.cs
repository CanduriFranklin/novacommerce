using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Sales.Domain
{
    public class Trace
    {
        [Key]
        public Guid TraceId { get; set; }

        [Required]
        public string AgentId { get; set; } // Identifier of the agent performing the action (e.g., user ID, service name)

        [Required]
        public string Operation { get; set; } // Action performed (e.g., "OrderCreated", "OrderConfirmed")

        [Required]
        public string Entity { get; set; } // Affected entity type (e.g., "Product", "Order")

        public Guid? EntityId { get; set; } // Reference to the affected entity's ID

        public DateTime Timestamp { get; set; }

        public string Details { get; set; } // Additional information (e.g., old/new values, reason)
    }
}
