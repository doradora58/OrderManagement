using OrderManagement.Models;

namespace OrderManagement.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly List<TransportOrder> _orders = new();
        public Task AddAsync(TransportOrder order)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TransportOrder>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<TransportOrder>>(_orders);
        }

        public Task UpdateStatusAsync(string id, string status)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = status;
            }
            return Task.CompletedTask;
        }
    }
}
