using ECommerce.Application.DTOs.Coupons;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Coupons;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class CouponTests
{
    private readonly ApplicationDbContext _context;
    private readonly CouponService _couponService;

    public CouponTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _couponService = new CouponService(_context, NullLogger<CouponService>.Instance);
    }

    [Fact]
    public async Task ValidateCouponAsync_PercentageWithCap_ShouldCalculateCorrectly()
    {
        // 20% off, max discount $30
        var coupon = new Coupon
        {
            Code = "SAVE20CAP",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20m,
            MaximumDiscountAmount = 30m,
            IsActive = true,
            StartsAtUtc = DateTime.UtcNow.AddDays(-1)
        };
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();

        // Case 1: Subtotal $100 -> 20% is $20 (under $30 cap) -> Discount = $20
        var result1 = await _couponService.ValidateCouponAsync(null, new ValidateCouponRequestDto
        {
            Code = "SAVE20CAP",
            OrderSubtotal = 100m
        });

        result1.Success.Should().BeTrue();
        result1.Data!.CalculatedDiscountAmount.Should().Be(20m);
        result1.Data.NewSubtotal.Should().Be(80m);

        // Case 2: Subtotal $250 -> 20% is $50 (exceeds $30 cap) -> Discount = $30
        var result2 = await _couponService.ValidateCouponAsync(null, new ValidateCouponRequestDto
        {
            Code = "SAVE20CAP",
            OrderSubtotal = 250m
        });

        result2.Success.Should().BeTrue();
        result2.Data!.CalculatedDiscountAmount.Should().Be(30m);
        result2.Data.NewSubtotal.Should().Be(220m);
    }

    [Fact]
    public async Task ValidateCouponAsync_MinimumSpendAndUsageLimits_ShouldEnforceRules()
    {
        var user = await _context.Users.FirstAsync();

        var coupon = new Coupon
        {
            Code = "VIPEXCLUSIVE",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 15m,
            MinimumOrderAmount = 50m,
            UsageLimitTotal = 2,
            UsageLimitPerCustomer = 1,
            CurrentUsageCount = 0,
            IsActive = true,
            StartsAtUtc = DateTime.UtcNow.AddDays(-1)
        };
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();

        // 1. Below minimum spend of $50
        var belowMinSpend = await _couponService.ValidateCouponAsync(user.Id, new ValidateCouponRequestDto
        {
            Code = "VIPEXCLUSIVE",
            OrderSubtotal = 40m
        });
        belowMinSpend.Success.Should().BeFalse();
        belowMinSpend.Message.Should().ContainEquivalentOf("Minimum spend");

        // 2. Valid spend
        var validSpend = await _couponService.ValidateCouponAsync(user.Id, new ValidateCouponRequestDto
        {
            Code = "VIPEXCLUSIVE",
            OrderSubtotal = 60m
        });
        validSpend.Success.Should().BeTrue();
        validSpend.Data!.CalculatedDiscountAmount.Should().Be(15m);
        validSpend.Data.NewSubtotal.Should().Be(45m);

        // 3. Record usage for user
        await _couponService.RecordUsageAsync("VIPEXCLUSIVE", Guid.NewGuid(), user.Id, 15m);

        // 4. Per customer limit reached for this user
        var secondUsage = await _couponService.ValidateCouponAsync(user.Id, new ValidateCouponRequestDto
        {
            Code = "VIPEXCLUSIVE",
            OrderSubtotal = 60m
        });
        secondUsage.Success.Should().BeFalse();
        secondUsage.Message.Should().Be("Usage Limit Reached");
        secondUsage.Errors.Should().Contain(e => e.Contains("maximum allowed times"));
    }
}
