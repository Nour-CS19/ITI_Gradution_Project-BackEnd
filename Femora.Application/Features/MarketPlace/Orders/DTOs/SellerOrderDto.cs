using System;
using System.Collections.Generic;

namespace Femora.Application.Features.MarketPlace.Orders.DTOs
{
    public record SellerOrderItemDto(
        Guid ProductVariantId,
        string ProductName,
        string VariantName,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal
    );

    public record SellerOrderDto(
        Guid Id,
        string OrderNumber,
        string CustomerFirstName,
        string CustomerLastName,
        string Status,
        decimal TotalAmount,
        DateTime CreatedAt,
        List<SellerOrderItemDto> Items
    );
}
