using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.DTOs.Reviews;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Orders;
using ECommerce.Infrastructure.Services.Reviews;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class ReviewTests
{
    private readonly ApplicationDbContext _context;
    private readonly ReviewService _reviewService;
    private readonly OrderService _orderService;

    public ReviewTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _reviewService = new ReviewService(_context, NullLogger<ReviewService>.Instance);
        _orderService = new OrderService(_context, new OrderStateMachine(), NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task SubmitReview_VerifiedPurchase_ShouldDetectAndSaveReview()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        // 1. Create a confirmed order for this user containing the product
        var orderRes = await _orderService.CreateOrderAsync(user.Id, new CreateOrderDto
        {
            CustomerName = "Reviewer Customer",
            CustomerEmail = user.Email,
            CustomerPhone = "+12025550133",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Reviewer",
                LastName = "User",
                StreetLine1 = "123 Review Way",
                City = "Boston",
                CountryCode = "US"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        });

        var order = await _context.Orders.FindAsync(orderRes.Data!.Id);
        order!.Status = OrderStatus.Delivered;
        await _context.SaveChangesAsync();

        // 2. Submit Review
        var submitRes = await _reviewService.SubmitReviewAsync(user.Id, new CreateProductReviewDto
        {
            ProductId = product.Id,
            Rating = 5,
            Title = "Outstanding Quality!",
            Comment = "This product exceeded my expectations in every aspect."
        });

        submitRes.Success.Should().BeTrue();
        submitRes.Data.Should().NotBeNull();
        submitRes.Data!.IsVerifiedPurchase.Should().BeTrue();
        submitRes.Data.Status.Should().Be(ReviewStatus.PendingModeration);
        submitRes.Data.Rating.Should().Be(5);
    }

    [Fact]
    public async Task ModerateReview_And_HelpfulVote_ShouldUpdateAggregateRatingAndVotes()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        var review = new ProductReview
        {
            ProductId = product.Id,
            UserId = user.Id,
            Rating = 5,
            Title = "Great product",
            Comment = "Loved the materials and finish.",
            Status = ReviewStatus.PendingModeration,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _context.ProductReviews.AddAsync(review);
        await _context.SaveChangesAsync();

        // 1. Moderate Review: Approve
        var moderateRes = await _reviewService.ModerateReviewAsync(review.Id, new ModerateReviewDto
        {
            Status = ReviewStatus.Approved
        });

        moderateRes.Success.Should().BeTrue();
        moderateRes.Data!.Status.Should().Be(ReviewStatus.Approved);

        // Verify product aggregate rating was recalculated
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        updatedProduct!.TotalReviews.Should().BeGreaterThan(0);
        updatedProduct.AverageRating.Should().BeGreaterThan(0);

        // 2. Helpful vote
        var voteRes = await _reviewService.VoteHelpfulAsync(review.Id);
        voteRes.Success.Should().BeTrue();

        var updatedReview = await _context.ProductReviews.FindAsync(review.Id);
        updatedReview!.HelpfulVotesCount.Should().Be(1);

        // 3. Summary Check
        var summaryRes = await _reviewService.GetProductReviewSummaryAsync(product.Id);
        summaryRes.Success.Should().BeTrue();
        summaryRes.Data!.FiveStarCount.Should().BeGreaterThan(0);
    }
}
