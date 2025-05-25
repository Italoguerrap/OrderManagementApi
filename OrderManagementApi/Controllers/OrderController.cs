using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.Requests;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        protected virtual IOrderService OrderService { get; set; } = orderService;

        [HttpPost("start")]
        public virtual async Task<IActionResult> StartOrderAsync(CancellationToken cancellationToken)
        {
            OrderDto order = await OrderService.StartOrderAsync(cancellationToken);
            return StatusCode(201, order);
        }

        [HttpPost("{orderId}/products")]
        public virtual async Task<IActionResult> AddProductAsync([FromRoute] long orderId, [FromBody] AddProductToOrderRequest request, [FromServices] IValidator<AddProductToOrderRequest> validator, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            OrderDto updatedOrder = await OrderService.AddProductToOrderAsync(orderId, request.ProductId, request.Quantity, cancellationToken);
            return Ok(updatedOrder);
        }

        [HttpDelete("{orderId}/products/{productId}")]
        public virtual async Task<IActionResult> RemoveProductAsync([FromRoute] long orderId,[FromRoute] long productId, CancellationToken cancellationToken)
        {
            OrderDto updatedOrder = await OrderService.RemoveProductFromOrderAsync(orderId, productId, cancellationToken);
            return Ok(updatedOrder);
        }

        [HttpPatch("{orderId}/close")]
        public virtual async Task<IActionResult> CloseOrderAsync([FromRoute] long orderId,CancellationToken cancellationToken)
        {
            OrderDto closedOrder = await OrderService.CloseOrderAsync(orderId, cancellationToken);
            return Ok(closedOrder);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllOrdersAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] OrderStatus? status = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<OrderDto> orders;
            if (status.HasValue)
                orders = await OrderService.GetOrdersByStatusAsync(status.Value, cancellationToken);
            else
                orders = await OrderService.GetAllOrdersAsync(cancellationToken);

            var totalCount = orders.Count();
            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = pagedOrders
            });
        }

        [HttpGet("{orderId}")]
        public virtual async Task<IActionResult> GetByIdAsync([FromRoute] long orderId, CancellationToken cancellationToken)
        {
            OrderDto? order = await OrderService.GetByIdAsync(orderId, cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
    }
}
