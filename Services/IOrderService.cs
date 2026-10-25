using OrderManagement.Models;

namespace OrderManagement.Services;

public interface IOrderService
{
    Task<TransportOrder> CreateOrderAsync(OrderRequest req);
    Task<IEnumerable<TransportOrder>> GetOrdersAsync();
}
