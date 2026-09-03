using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.International;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class InternationalApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InternationalApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConvertCurrency_ApiEndpoint_ShouldReturnConvertedValue()
    {
        var request = new ConvertCurrencyRequestDto
        {
            Amount = 50.00m,
            FromCurrency = "USD",
            ToCurrency = "PKR"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/currencies/convert", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ConvertCurrencyResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.ConvertedAmount.Should().BeGreaterThan(0);
        result.Data.ToCurrency.Should().Be("PKR");
    }

    [Fact]
    public async Task CalculateTax_ApiEndpoint_ShouldReturnAppliedRules()
    {
        var request = new CalculateTaxRequestDto
        {
            CountryCode = "US",
            SubtotalAmount = 200.00m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/tax-rules/calculate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CalculateTaxResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.TotalTaxAmount.Should().BeGreaterThan(0);
        result.Data.AppliedRules.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCountries_ApiEndpoint_ShouldReturnCountryList()
    {
        var response = await _client.GetAsync("/api/v1/currencies/countries");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CountryDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().Contain(c => c.IsoCode == "PK");
        result.Data.Should().Contain(c => c.IsoCode == "US");
    }
}
