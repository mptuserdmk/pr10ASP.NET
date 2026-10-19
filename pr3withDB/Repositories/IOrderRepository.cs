using System.Threading.Tasks;
using pr3withDB.Common;
using pr3withDB.Models;

namespace pr3withDB.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<PagedResult<Order>> GetOrdersWithDetailsPagedAsync(int page, int pageSize, int? userId = null);
    }
}
