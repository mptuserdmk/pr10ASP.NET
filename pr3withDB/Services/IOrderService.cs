using System.Threading.Tasks;
using pr3withDB.Common;
using pr3withDB.DTOs;

namespace pr3withDB.Services
{
    public interface IOrderService
    {
        Task<ServiceResult<PagedResult<OrderReadDto>>> GetPagedOrdersAsync(int page, int pageSize, int? userId = null);
        Task<ServiceResult<OrderReadDto>> GetOrderByIdAsync(int orderId);
        Task<ServiceResult<OrderReadDto>> CreateOrderAsync(CreateOrderDto dto);
        Task<ServiceResult<OrderReadDto>> CancelOrderAsync(int orderId);
        Task<ServiceResult<bool>> DeleteOrderAsync(int orderId);
    }
}
