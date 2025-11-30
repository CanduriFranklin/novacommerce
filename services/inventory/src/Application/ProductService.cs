using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NovaCommerce.Inventory.Domain;
using NovaCommerce.Inventory.Infrastructure;

namespace NovaCommerce.Inventory.Application
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ImageService _imageService;
        private readonly InventoryDbContext _dbContext;
        private readonly IProductCacheService _productCacheService;

        public ProductService(IProductRepository productRepository, ImageService imageService, InventoryDbContext dbContext, IProductCacheService productCacheService)
        {
            _productRepository = productRepository;
            _imageService = imageService;
            _dbContext = dbContext;
            _productCacheService = productCacheService;
        }

        public async Task<Product> GetProductById(Guid id)
        {
            var product = await _productCacheService.GetProductAsync(id);
            if (product == null)
            {
                product = await _productRepository.GetByIdAsync(id);
                if (product != null)
                {
                    await _productCacheService.SetProductAsync(product);
                }
            }
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            var products = await _productCacheService.GetProductsAsync();
            if (products == null)
            {
                products = await _productRepository.GetAllAsync();
                if (products != null)
                {
                    await _productCacheService.SetProductsAsync(products);
                }
            }
            return products;
        }

        public async Task AddProduct(Product product, IFormFile image)
        {
            if (image != null)
            {
                var imageUrl = await _imageService.UploadImageAsync(image);
                product.SetImageUrl(imageUrl);
            }

            await _productRepository.AddAsync(product);

            await _dbContext.StockMovements.AddAsync(new StockMovement
            {
                ProductId = product.ProductId,
                QuantityChanged = product.Stock,
                Reason = "Initial stock",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            await _dbContext.Traces.AddAsync(new Trace
            {
                AgentId = "System",
                Operation = "ProductAdded",
                Entity = "Product",
                EntityId = product.ProductId,
                Timestamp = DateTime.UtcNow,
                Details = $"Product '{product.Name}' added with initial stock {product.Stock}."
            });
            await _dbContext.SaveChangesAsync();

            await _productCacheService.SetProductAsync(product);
        }

        public async Task UpdateProduct(Product product, IFormFile image)
        {
            var existingProduct = await _productRepository.GetByIdAsync(product.ProductId);
            if (existingProduct == null)
            {
                throw new ApplicationException($"Product with ID {product.ProductId} not found.");
            }

            var oldImageUrl = existingProduct.ImageUrl;
            var stockDifference = product.Stock - existingProduct.Stock;

            existingProduct.UpdateDetails(product.Name, product.Description, product.Price);

            if (stockDifference != 0)
            {
                existingProduct.UpdateStock(stockDifference, "Stock updated");
            }

            if (image != null)
            {
                var newImageUrl = await _imageService.UploadImageAsync(image);
                existingProduct.SetImageUrl(newImageUrl);

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await _imageService.DeleteImageAsync(oldImageUrl);
                }
            }

            await _productRepository.UpdateAsync(existingProduct);

            if (stockDifference != 0)
            {
                await _dbContext.StockMovements.AddAsync(new StockMovement
                {
                    ProductId = product.ProductId,
                    QuantityChanged = stockDifference,
                    Reason = "Stock updated",
                    CreatedAt = DateTime.UtcNow
                });
                await _dbContext.SaveChangesAsync();

                await _dbContext.Traces.AddAsync(new Trace
                {
                    AgentId = "System",
                    Operation = "StockUpdated",
                    Entity = "Product",
                    EntityId = product.ProductId,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Stock for product '{product.Name}' changed by {stockDifference}. New stock: {product.Stock}."
                });
                await _dbContext.SaveChangesAsync();
            }
            await _productCacheService.SetProductAsync(existingProduct);
        }

        public async Task DeleteProduct(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(product.ImageUrl);
                }
                await _productRepository.DeleteAsync(id);
                await _productCacheService.RemoveProductAsync(id);

                await _dbContext.Traces.AddAsync(new Trace
                {
                    AgentId = "System",
                    Operation = "ProductDeleted",
                    Entity = "Product",
                    EntityId = product.ProductId,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Product '{product.Name}' deleted."
                });
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
