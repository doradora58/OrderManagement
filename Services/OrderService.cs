using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagement.Services
{
    public class OrderService(IOrderRepository orderRepository, HttpClient httpClient, IConfiguration configuration) : IOrderService
    {
 
        public async Task<TransportOrder> CreateOrderAsync(OrderRequest req)
        {
            var order = new TransportOrder
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                From = req.From,
                To = req.To,
                Status = "CREATED",
                CreatedAt = DateTime.Now
            };

            await orderRepository.AddAsync(order);

            var fmsUrl = configuration["FMS_API_URL"] ?? "http://fms:8080/api/dispatch";
            try
            {
                var response = await httpClient.PostAsJsonAsync(fmsUrl, new
                {
                    orderId = order.Id,
                    fromAddress = $"{order.From.PostalCode} {order.From.Address}",
                    toAddress = $"{order.To.PostalCode} {order.To.Address}"
                });

                var newStatus = response.IsSuccessStatusCode ? "DISPATCHED" : "DISPATCH_FAILED";
                await orderRepository.UpdateStatusAsync(order.Id, newStatus);
                order.Status = newStatus;
            }
            catch
            {
                await orderRepository.UpdateStatusAsync(order.Id, "FMS_UNREACHABLE");
                order.Status = "FMS_UNREACHABLE";
            }

            return order;
        }

        public async Task<IEnumerable<TransportOrder>> GetOrdersAsync()
        {
            return await orderRepository.GetAllAsync();
        }
    }
}
