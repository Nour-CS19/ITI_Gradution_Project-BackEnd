namespace Femora.Application.Features.MarketPlace.Products.DTOs
{
    public record SellerStatsDto
    {
        public int TotalProducts { get; init; }
        public int DraftProducts { get; init; }
        public int PendingProducts { get; init; }
        public int ApprovedProducts { get; init; }
        public int RejectedProducts { get; init; }

        public int TotalOrders { get; init; }
        public int PendingOrders { get; init; }
        public int ProcessingOrders { get; init; }
        public int ShippedOrders { get; init; }
        public int DeliveredOrders { get; init; }

        public decimal TotalRevenue { get; init; }

        public List<BestSellerProductDto> BestSellingProducts { get; init; } = [];
        public List<SellerRecentOrderDto> LatestOrders { get; init; } = [];
    }

    public record BestSellerProductDto(
        Guid ProductId,
        string ProductName,
        string? ImageUrl,
        int TotalSold,
        decimal Revenue
    );

    public record SellerRecentOrderDto(
        Guid OrderId,
        string OrderNumber,
        string CustomerName,
        string Status,
        decimal Amount,
        DateTime CreatedAt
    );
}
