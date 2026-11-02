using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Create a new order from shopping basket (Checkout).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OrderDto>>> CreateOrder(
            [FromBody] CreateOrderDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _orderService.CreateOrderAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = created.Id },
                ApiResponseDto<OrderDto>.Created(created, "Order created successfully."));
        }

        /// <summary>
        /// Get all orders placed by the current authenticated customer.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OrderDto>>>> GetMyOrders(
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderService.GetMyOrdersAsync(cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<OrderDto>>.Ok(orders));
        }

        /// <summary>
        /// Get specific order details by ID (Customer can view their own; Admin can view any).
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponseDto<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OrderDto>>> GetOrderById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            return Ok(ApiResponseDto<OrderDto>.Ok(order));
        }

        /// <summary>
        /// Get all customer orders with pagination (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResponseDto<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<OrderDto>>>> GetAllOrders(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderService.GetAllOrdersAsync(pageIndex, pageSize, cancellationToken);
            return Ok(ApiResponseDto<PaginatedResponseDto<OrderDto>>.Ok(orders));
        }

        /// <summary>
        /// Update order status (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponseDto<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OrderDto>>> UpdateOrderStatus(
            [FromRoute] Guid id,
            [FromBody] UpdateOrderStatusDto dto,
            CancellationToken cancellationToken = default)
        {
            var updated = await _orderService.UpdateOrderStatusAsync(id, dto.Status, cancellationToken);
            return Ok(ApiResponseDto<OrderDto>.Ok(updated, $"Order status updated to '{updated.Status}'."));
        }

        /// <summary>
        /// Cancel an order (Restores reserved stock back to inventory).
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponseDto<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OrderDto>>> CancelOrder(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var cancelled = await _orderService.CancelOrderAsync(id, cancellationToken);
            return Ok(ApiResponseDto<OrderDto>.Ok(cancelled, "Order cancelled successfully and inventory restored."));
        }
    }
}

