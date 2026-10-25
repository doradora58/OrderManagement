using OrderManagement.Models;

namespace OrderManagement.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(TransportOrder order);
        Task<IEnumerable<TransportOrder>> GetAllAsync();
        Task UpdateStatusAsync(string id, string status);
    }
}
