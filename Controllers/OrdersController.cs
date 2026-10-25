using Microsoft.AspNetCore.Mvc;
using OrderManagement.Models;
using OrderManagement.Services;

namespace OrderManagement.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest req)
    {
        var order = await _orderService.CreateOrderAsync(req);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }
}
