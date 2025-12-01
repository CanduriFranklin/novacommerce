using System;
using NovaCommerce.Inventory.Domain;
using Xunit;

namespace NovaCommerce.Inventory.UnitTests
{
    public class ProductTests
    {
        [Fact]
        public void Product_Creation_Should_Set_Properties_Correctly()
        {
            // Arrange
            var name = "Test Product";
            var description = "Test Description";
            var price = 10.0m;
            var stock = 100;

            // Act
            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock
            };

            // Assert
            Assert.Equal(name, product.Name);
            Assert.Equal(description, product.Description);
            Assert.Equal(price, product.Price);
            Assert.Equal(stock, product.Stock);
        }
    }
}
