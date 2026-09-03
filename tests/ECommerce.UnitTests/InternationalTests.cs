using ECommerce.Application.DTOs.International;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.International;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class InternationalTests
{
    private readonly ApplicationDbContext _context;
    private readonly CurrencyService _currencyService;
    private readonly TaxService _taxService;

    public InternationalTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _currencyService = new CurrencyService(_context, NullLogger<CurrencyService>.Instance);
        _taxService = new TaxService(_context, NullLogger<TaxService>.Instance);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_USD_To_PKR_ShouldComputeAccurately()
    {
        // 100 USD at 280.00 PKR/USD -> 28,000 PKR
        var response = await _currencyService.ConvertCurrencyAsync(new ConvertCurrencyRequestDto
        {
            Amount = 100m,
            FromCurrency = "USD",
            ToCurrency = "PKR"
        });

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ConvertedAmount.Should().Be(28000.00m);
        response.Data.FromCurrency.Should().Be("USD");
        response.Data.ToCurrency.Should().Be("PKR");
    }

    [Fact]
    public async Task ConvertCurrencyAsync_EUR_To_GBP_CrossRate_ShouldComputeAccurately()
    {
        // EUR RateToBase = 0.92, GBP RateToBase = 0.79
        // 100 EUR -> (100 / 0.92) * 0.79 = 85.87 GBP
        var response = await _currencyService.ConvertCurrencyAsync(new ConvertCurrencyRequestDto
        {
            Amount = 100m,
            FromCurrency = "EUR",
            ToCurrency = "GBP"
        });

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ConvertedAmount.Should().Be(85.87m);
    }

    [Fact]
    public async Task CalculateTaxAsync_RegionalRule_ShouldComputeTax()
    {
        // PK Standard Sales Tax is 17% (0.17)
        var response = await _taxService.CalculateTaxAsync(new CalculateTaxRequestDto
        {
            CountryCode = "PK",
            SubtotalAmount = 1000m
        });

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.TotalTaxAmount.Should().Be(170.00m);
        response.Data.EffectiveTaxRate.Should().Be(0.17m);
        response.Data.AppliedRules.Should().NotBeEmpty();
    }
}
