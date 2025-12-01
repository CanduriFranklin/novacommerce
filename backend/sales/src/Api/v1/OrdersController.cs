using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaCommerce.Sales.Application;
using NovaCommerce.Sales.Application.Dtos;
using NovaCommerce.Sales.Domain;

namespace NovaCommerce.Sales.Api.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize] // All actions require authentication by default
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }
            var orderDto = new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal
                }).ToList()
            };
            return orderDto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _orderService.GetAllOrders();
            var orderDtos = new List<OrderDto>();
            foreach (var order in orders)
            {
                orderDtos.Add(new OrderDto
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    Status = order.Status.ToString(),
                    Total = order.Total,
                    CreatedAt = order.CreatedAt,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        LineTotal = oi.LineTotal
                    }).ToList()
                });
            }
            return Ok(orderDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")] // Only Customer can create orders
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                CustomerId = createOrderDto.CustomerId,
                OrderItems = createOrderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.Quantity * oi.UnitPrice
                }).ToList(),
                Total = createOrderDto.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
            };

            await _orderService.CreateOrder(order);

            var orderDto = new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId, version = "1.0" }, orderDto);
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Admin")] // Only Admin can confirm orders
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            var result = await _orderService.ConfirmOrder(id);
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Order could not be confirmed.");
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin")] // Only Admin can cancel orders
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var result = await _orderService.CancelOrder(id);
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Order could not be cancelled.");
            }
        }
    }
}
