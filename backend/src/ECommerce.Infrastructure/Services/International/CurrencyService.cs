using ECommerce.Application.DTOs.International;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.International;

public class CurrencyService : ICurrencyService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(ApplicationDbContext context, ILogger<CurrencyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<CurrencyDto>>> GetAllCurrenciesAsync(bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Currencies.AsQueryable();
        if (isActiveOnly.HasValue && isActiveOnly.Value)
        {
            query = query.Where(c => c.IsActive);
        }

        var currencies = await query.OrderBy(c => c.Code).ToListAsync(cancellationToken);
        var dtos = currencies.Select(c => MapToCurrencyDto(c)).ToList();
        return ApiResponse<IReadOnlyList<CurrencyDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<CurrencyDto>> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);
        if (currency == null)
        {
            return ApiResponse<CurrencyDto>.FailureResponse("Not Found", $"Currency '{normalized}' not found.");
        }

        return ApiResponse<CurrencyDto>.SuccessResponse(MapToCurrencyDto(currency));
    }

    public async Task<ApiResponse<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyDto request, CancellationToken cancellationToken = default)
    {
        var normalized = request.Code.Trim().ToUpperInvariant();
        var exists = await _context.Currencies.AnyAsync(c => c.Code == normalized, cancellationToken);
        if (exists)
        {
            return ApiResponse<CurrencyDto>.FailureResponse("Conflict", $"Currency '{normalized}' already exists.");
        }

        if (request.IsBaseCurrency)
        {
            // Reset existing base currencies
            var existingBases = await _context.Currencies.Where(c => c.IsBaseCurrency).ToListAsync(cancellationToken);
            foreach (var b in existingBases)
            {
                b.IsBaseCurrency = false;
            }
        }

        var currency = new Currency
        {
            Code = normalized,
            Name = request.Name.Trim(),
            Symbol = request.Symbol.Trim(),
            ExchangeRateToBase = request.ExchangeRateToBase,
            IsBaseCurrency = request.IsBaseCurrency,
            IsActive = request.IsActive,
            DecimalPlaces = request.DecimalPlaces
        };

        await _context.Currencies.AddAsync(currency, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created currency {CurrencyCode} (Rate: {Rate})", currency.Code, currency.ExchangeRateToBase);

        return ApiResponse<CurrencyDto>.SuccessResponse(MapToCurrencyDto(currency));
    }

    public async Task<ApiResponse<CurrencyDto>> UpdateExchangeRateAsync(string code, UpdateCurrencyRateDto request, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);
        if (currency == null)
        {
            return ApiResponse<CurrencyDto>.FailureResponse("Not Found", $"Currency '{normalized}' not found.");
        }

        currency.ExchangeRateToBase = request.ExchangeRateToBase;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated exchange rate for {CurrencyCode} to {NewRate}", currency.Code, currency.ExchangeRateToBase);

        return ApiResponse<CurrencyDto>.SuccessResponse(MapToCurrencyDto(currency));
    }

    public async Task<ApiResponse<ConvertCurrencyResponseDto>> ConvertCurrencyAsync(ConvertCurrencyRequestDto request, CancellationToken cancellationToken = default)
    {
        var fromCode = request.FromCurrency.Trim().ToUpperInvariant();
        var toCode = request.ToCurrency.Trim().ToUpperInvariant();

        if (fromCode == toCode)
        {
            return ApiResponse<ConvertCurrencyResponseDto>.SuccessResponse(new ConvertCurrencyResponseDto
            {
                OriginalAmount = request.Amount,
                FromCurrency = fromCode,
                ConvertedAmount = request.Amount,
                ToCurrency = toCode,
                ExchangeRate = 1.0m,
                ConvertedAtUtc = DateTime.UtcNow
            });
        }

        var fromCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == fromCode && c.IsActive, cancellationToken);
        if (fromCurrency == null)
        {
            return ApiResponse<ConvertCurrencyResponseDto>.FailureResponse("Not Found", $"Source currency '{fromCode}' is not supported or active.");
        }

        var toCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == toCode && c.IsActive, cancellationToken);
        if (toCurrency == null)
        {
            return ApiResponse<ConvertCurrencyResponseDto>.FailureResponse("Not Found", $"Destination currency '{toCode}' is not supported or active.");
        }

        // Conversion formula: AmountInBase = OriginalAmount / FromRateToBase; ConvertedAmount = AmountInBase * ToRateToBase
        var amountInBase = request.Amount / fromCurrency.ExchangeRateToBase;
        var converted = Math.Round(amountInBase * toCurrency.ExchangeRateToBase, toCurrency.DecimalPlaces);
        var directRate = Math.Round(toCurrency.ExchangeRateToBase / fromCurrency.ExchangeRateToBase, 6);

        var response = new ConvertCurrencyResponseDto
        {
            OriginalAmount = request.Amount,
            FromCurrency = fromCode,
            ConvertedAmount = converted,
            ToCurrency = toCode,
            ExchangeRate = directRate,
            ConvertedAtUtc = DateTime.UtcNow
        };

        return ApiResponse<ConvertCurrencyResponseDto>.SuccessResponse(response);
    }

    public async Task<ApiResponse<IReadOnlyList<CountryDto>>> GetAllCountriesAsync(bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Countries.AsQueryable();
        if (isActiveOnly.HasValue && isActiveOnly.Value)
        {
            query = query.Where(c => c.IsActive);
        }

        var countries = await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        var dtos = countries.Select(c => new CountryDto
        {
            Id = c.Id,
            IsoCode = c.IsoCode,
            Name = c.Name,
            PhonePrefix = c.PhonePrefix,
            DefaultCurrency = c.DefaultCurrency,
            IsShippingAllowed = c.IsShippingAllowed,
            IsBillingAllowed = c.IsBillingAllowed,
            IsActive = c.IsActive
        }).ToList();

        return ApiResponse<IReadOnlyList<CountryDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<CountryDto>> CreateCountryAsync(CreateCountryDto request, CancellationToken cancellationToken = default)
    {
        var normalized = request.IsoCode.Trim().ToUpperInvariant();
        var exists = await _context.Countries.AnyAsync(c => c.IsoCode == normalized, cancellationToken);
        if (exists)
        {
            return ApiResponse<CountryDto>.FailureResponse("Conflict", $"Country '{normalized}' already exists.");
        }

        var country = new Country
        {
            IsoCode = normalized,
            Name = request.Name.Trim(),
            PhonePrefix = request.PhonePrefix?.Trim(),
            DefaultCurrency = request.DefaultCurrency.Trim().ToUpperInvariant(),
            IsShippingAllowed = request.IsShippingAllowed,
            IsBillingAllowed = request.IsBillingAllowed,
            IsActive = request.IsActive
        };

        await _context.Countries.AddAsync(country, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CountryDto>.SuccessResponse(new CountryDto
        {
            Id = country.Id,
            IsoCode = country.IsoCode,
            Name = country.Name,
            PhonePrefix = country.PhonePrefix,
            DefaultCurrency = country.DefaultCurrency,
            IsShippingAllowed = country.IsShippingAllowed,
            IsBillingAllowed = country.IsBillingAllowed,
            IsActive = country.IsActive
        });
    }

    private static CurrencyDto MapToCurrencyDto(Currency c)
    {
        return new CurrencyDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Symbol = c.Symbol,
            ExchangeRateToBase = c.ExchangeRateToBase,
            IsBaseCurrency = c.IsBaseCurrency,
            IsActive = c.IsActive,
            DecimalPlaces = c.DecimalPlaces
        };
    }
}
