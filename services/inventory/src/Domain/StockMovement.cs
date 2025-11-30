using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Domain
{
    public class StockMovement
    {
        [Key]
        public Guid StockMovementId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int QuantityChanged { get; set; }

        [Required]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
