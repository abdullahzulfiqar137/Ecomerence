using ECommerce.Application.DTOs.International;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ICurrencyService
{
    Task<ApiResponse<IReadOnlyList<CurrencyDto>>> GetAllCurrenciesAsync(bool? isActiveOnly = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CurrencyDto>> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ApiResponse<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CurrencyDto>> UpdateExchangeRateAsync(string code, UpdateCurrencyRateDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ConvertCurrencyResponseDto>> ConvertCurrencyAsync(ConvertCurrencyRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<CountryDto>>> GetAllCountriesAsync(bool? isActiveOnly = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CountryDto>> CreateCountryAsync(CreateCountryDto request, CancellationToken cancellationToken = default);
}

public interface ITaxService
{
    Task<ApiResponse<IReadOnlyList<TaxRuleDto>>> GetAllTaxRulesAsync(bool? isActiveOnly = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<TaxRuleDto>> GetTaxRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<TaxRuleDto>> CreateTaxRuleAsync(CreateTaxRuleDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TaxRuleDto>> UpdateTaxRuleAsync(Guid id, UpdateTaxRuleDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteTaxRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CalculateTaxResponseDto>> CalculateTaxAsync(CalculateTaxRequestDto request, CancellationToken cancellationToken = default);
}
