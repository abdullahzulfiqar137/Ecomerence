using ECommerce.Application.DTOs.International;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.International;

public class TaxService : ITaxService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaxService> _logger;

    public TaxService(ApplicationDbContext context, ILogger<TaxService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<TaxRuleDto>>> GetAllTaxRulesAsync(bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TaxRules.AsQueryable();
        if (isActiveOnly.HasValue && isActiveOnly.Value)
        {
            query = query.Where(t => t.IsActive);
        }

        var rules = await query.OrderBy(t => t.CountryCode).ThenBy(t => t.Priority).ToListAsync(cancellationToken);
        var dtos = rules.Select(t => MapToDto(t)).ToList();
        return ApiResponse<IReadOnlyList<TaxRuleDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<TaxRuleDto>> GetTaxRuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.TaxRules.FindAsync(new object[] { id }, cancellationToken);
        if (rule == null)
        {
            return ApiResponse<TaxRuleDto>.FailureResponse("Not Found", "Tax rule not found.");
        }

        return ApiResponse<TaxRuleDto>.SuccessResponse(MapToDto(rule));
    }

    public async Task<ApiResponse<TaxRuleDto>> CreateTaxRuleAsync(CreateTaxRuleDto request, CancellationToken cancellationToken = default)
    {
        var rule = new TaxRule
        {
            Name = request.Name.Trim(),
            CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
            StateOrProvince = request.StateOrProvince?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            Rate = request.Rate,
            Type = request.Type,
            IsActive = request.IsActive,
            Priority = request.Priority
        };

        await _context.TaxRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created tax rule {RuleName} for {Country} ({Rate:P1})", rule.Name, rule.CountryCode, rule.Rate);

        return ApiResponse<TaxRuleDto>.SuccessResponse(MapToDto(rule));
    }

    public async Task<ApiResponse<TaxRuleDto>> UpdateTaxRuleAsync(Guid id, UpdateTaxRuleDto request, CancellationToken cancellationToken = default)
    {
        var rule = await _context.TaxRules.FindAsync(new object[] { id }, cancellationToken);
        if (rule == null)
        {
            return ApiResponse<TaxRuleDto>.FailureResponse("Not Found", "Tax rule not found.");
        }

        rule.Name = request.Name.Trim();
        rule.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
        rule.StateOrProvince = request.StateOrProvince?.Trim();
        rule.PostalCode = request.PostalCode?.Trim();
        rule.Rate = request.Rate;
        rule.Type = request.Type;
        rule.IsActive = request.IsActive;
        rule.Priority = request.Priority;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TaxRuleDto>.SuccessResponse(MapToDto(rule));
    }

    public async Task<ApiResponse<bool>> DeleteTaxRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.TaxRules.FindAsync(new object[] { id }, cancellationToken);
        if (rule == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Tax rule not found.");
        }

        _context.TaxRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Tax rule deleted successfully.");
    }

    public async Task<ApiResponse<CalculateTaxResponseDto>> CalculateTaxAsync(CalculateTaxRequestDto request, CancellationToken cancellationToken = default)
    {
        var country = request.CountryCode.Trim().ToUpperInvariant();
        var state = request.StateOrProvince?.Trim();

        var query = _context.TaxRules
            .Where(t => t.CountryCode == country && t.IsActive)
            .OrderBy(t => t.Priority)
            .AsQueryable();

        var rules = await query.ToListAsync(cancellationToken);

        // Filter matching rules (State-specific or Country-wide)
        var applicableRules = rules.Where(r =>
            string.IsNullOrEmpty(r.StateOrProvince) ||
            (!string.IsNullOrEmpty(state) && r.StateOrProvince.Equals(state, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        decimal totalTax = 0m;
        var appliedDetails = new List<AppliedTaxRuleDetailDto>();

        foreach (var rule in applicableRules)
        {
            var ruleTax = Math.Round(request.SubtotalAmount * rule.Rate, 2);
            totalTax += ruleTax;

            appliedDetails.Add(new AppliedTaxRuleDetailDto
            {
                RuleName = rule.Name,
                Rate = rule.Rate,
                TaxAmount = ruleTax
            });
        }

        var effectiveRate = request.SubtotalAmount > 0
            ? Math.Round(totalTax / request.SubtotalAmount, 4)
            : 0m;

        var response = new CalculateTaxResponseDto
        {
            SubtotalAmount = request.SubtotalAmount,
            TotalTaxAmount = totalTax,
            EffectiveTaxRate = effectiveRate,
            AppliedRules = appliedDetails
        };

        return ApiResponse<CalculateTaxResponseDto>.SuccessResponse(response);
    }

    private static TaxRuleDto MapToDto(TaxRule t)
    {
        return new TaxRuleDto
        {
            Id = t.Id,
            Name = t.Name,
            CountryCode = t.CountryCode,
            StateOrProvince = t.StateOrProvince,
            PostalCode = t.PostalCode,
            Rate = t.Rate,
            Type = t.Type,
            IsActive = t.IsActive,
            Priority = t.Priority
        };
    }
}
