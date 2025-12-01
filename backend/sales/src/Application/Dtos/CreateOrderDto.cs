using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Sales.Application.Dtos
{
    public class CreateOrderDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public List<CreateOrderItemDto> OrderItems { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
