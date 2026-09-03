using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Services.Orders;

public class OrderStateMachine : IOrderStateMachine
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = new() { OrderStatus.Confirmed, OrderStatus.Cancelled, OrderStatus.Failed },
        [OrderStatus.Confirmed] = new() { OrderStatus.Processing, OrderStatus.Cancelled },
        [OrderStatus.Processing] = new() { OrderStatus.Packed, OrderStatus.Cancelled },
        [OrderStatus.Packed] = new() { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new() { OrderStatus.OutForDelivery, OrderStatus.Delivered, OrderStatus.Returned },
        [OrderStatus.OutForDelivery] = new() { OrderStatus.Delivered, OrderStatus.Returned },
        [OrderStatus.Delivered] = new() { OrderStatus.Returned, OrderStatus.Refunded },
        [OrderStatus.Cancelled] = new(),
        [OrderStatus.Returned] = new() { OrderStatus.Refunded },
        [OrderStatus.Refunded] = new(),
        [OrderStatus.Failed] = new()
    };

    public bool CanTransition(OrderStatus currentStatus, OrderStatus targetStatus)
    {
        if (currentStatus == targetStatus)
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(targetStatus);
    }

    public IReadOnlyList<OrderStatus> GetAllowedTransitions(OrderStatus currentStatus)
    {
        if (AllowedTransitions.TryGetValue(currentStatus, out var allowed))
        {
            return allowed.ToList();
        }

        return Array.Empty<OrderStatus>();
    }
}
