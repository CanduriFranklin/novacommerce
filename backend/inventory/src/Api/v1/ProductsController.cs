using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaCommerce.Inventory.Application;
using NovaCommerce.Inventory.Application.Dtos;
using NovaCommerce.Inventory.Domain;

namespace NovaCommerce.Inventory.Api.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize] // All actions require authentication by default
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            var productDto = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl
            };
            return productDto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productService.GetAllProducts();
            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                productDtos.Add(new ProductDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl
                });
            }
            return Ok(productDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can create products
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto, IFormFile image)
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock
            };

            await _productService.AddProduct(product, image);

            var productDto = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId, version = "1.0" }, productDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can update products
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] CreateProductDto updateProductDto, IFormFile image) // Changed to CreateProductDto for simplicity, ideally a dedicated UpdateProductDto
        {
            // Fetch existing product
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            // Update properties from DTO
            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.Stock = updateProductDto.Stock;

            await _productService.UpdateProduct(product, image);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete products
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}
