using System;
using System.Collections.Generic;
using NovaCommerce.Sales.Domain;
using Xunit;

namespace NovaCommerce.Sales.UnitTests
{
    public class OrderTests
    {
        [Fact]
        public void Order_Creation_Should_Set_Properties_Correctly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.0m }
            };

            // Act
            var order = new Order
            {
                CustomerId = customerId,
                OrderItems = orderItems,
                Total = 10.0m
            };

            // Assert
            Assert.Equal(customerId, order.CustomerId);
            Assert.Equal(orderItems, order.OrderItems);
            Assert.Equal(10.0m, order.Total);
        }
    }
}
