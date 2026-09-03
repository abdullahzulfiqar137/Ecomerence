using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

public interface IOrderStateMachine
{
    bool CanTransition(OrderStatus currentStatus, OrderStatus targetStatus);
    IReadOnlyList<OrderStatus> GetAllowedTransitions(OrderStatus currentStatus);
}

public interface IOrderService
{
    Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(OrderFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid orderId, Guid? requestingUserId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> GetOrderByNumberAsync(string orderNumber, Guid? requestingUserId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid? userId, CreateOrderDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto request, string? changedBy = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> CancelOrderAsync(Guid orderId, CancelOrderRequestDto request, Guid? requestingUserId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderNoteDto>> AddOrderNoteAsync(Guid orderId, AddOrderNoteDto request, string? authorName = null, CancellationToken cancellationToken = default);
}
