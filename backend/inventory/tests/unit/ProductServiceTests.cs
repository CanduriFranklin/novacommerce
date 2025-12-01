using System;
using System.Threading.Tasks;
using Moq;
using NovaCommerce.Inventory.Application;
using NovaCommerce.Inventory.Domain;
using NovaCommerce.Inventory.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace NovaCommerce.Inventory.UnitTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ImageService> _imageServiceMock;
        private readonly Mock<InventoryDbContext> _dbContextMock;
        private readonly Mock<IProductCacheService> _productCacheServiceMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _imageServiceMock = new Mock<ImageService>(new Mock<IBlobStorageService>().Object);
            var options = new DbContextOptions<InventoryDbContext>();
            _dbContextMock = new Mock<InventoryDbContext>(options);
            _productCacheServiceMock = new Mock<IProductCacheService>();
            _productService = new ProductService(_productRepositoryMock.Object, _imageServiceMock.Object, _dbContextMock.Object, _productCacheServiceMock.Object);
        }

        [Fact]
        public async Task GetProductById_Should_Return_Product()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product("Test Product", "Description", 10, 100);
            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductById(productId);

            // Assert
            Assert.Equal(product.Name, result.Name);
        }
    }
}
