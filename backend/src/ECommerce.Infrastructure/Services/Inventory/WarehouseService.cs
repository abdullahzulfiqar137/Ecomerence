using ECommerce.Application.DTOs.Inventory;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Inventory;

public class WarehouseService : IWarehouseService
{
    private readonly IGenericRepository<Warehouse> _warehouseRepo;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IGenericRepository<Warehouse> warehouseRepo,
        ApplicationDbContext context,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepo = warehouseRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<WarehouseDto>>> GetAllWarehousesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Warehouses
            .Include(w => w.InventoryItems)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        var warehouses = await query
            .OrderByDescending(w => w.IsPrimary)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);

        var dtos = warehouses.Select(w => MapToDto(w)).ToList();
        return ApiResponse<IReadOnlyList<WarehouseDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.InventoryItems)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse == null)
        {
            return ApiResponse<WarehouseDto>.FailureResponse("Not Found", "Warehouse not found.");
        }

        return ApiResponse<WarehouseDto>.SuccessResponse(MapToDto(warehouse));
    }

    public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto request, CancellationToken cancellationToken = default)
    {
        if (await _context.Warehouses.AnyAsync(w => w.Code == request.Code.Trim(), cancellationToken))
        {
            return ApiResponse<WarehouseDto>.FailureResponse("Conflict", $"Warehouse code '{request.Code}' already exists.");
        }

        if (request.IsPrimary)
        {
            var existingPrimaries = await _context.Warehouses.Where(w => w.IsPrimary).ToListAsync(cancellationToken);
            foreach (var p in existingPrimaries)
            {
                p.IsPrimary = false;
            }
        }

        var warehouse = new Warehouse
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            ContactPerson = request.ContactPerson,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            StreetLine1 = request.StreetLine1.Trim(),
            StreetLine2 = request.StreetLine2,
            City = request.City.Trim(),
            StateOrProvince = request.StateOrProvince.Trim(),
            PostalCode = request.PostalCode.Trim(),
            CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
            IsActive = request.IsActive,
            IsPrimary = request.IsPrimary
        };

        await _warehouseRepo.AddAsync(warehouse, cancellationToken);
        await _warehouseRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created warehouse {WarehouseName} ({WarehouseCode})", warehouse.Name, warehouse.Code);

        return await GetWarehouseByIdAsync(warehouse.Id, cancellationToken);
    }

    public async Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid id, UpdateWarehouseDto request, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepo.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return ApiResponse<WarehouseDto>.FailureResponse("Not Found", "Warehouse not found.");
        }

        if (request.IsPrimary && !warehouse.IsPrimary)
        {
            var existingPrimaries = await _context.Warehouses.Where(w => w.IsPrimary && w.Id != id).ToListAsync(cancellationToken);
            foreach (var p in existingPrimaries)
            {
                p.IsPrimary = false;
            }
        }

        warehouse.Name = request.Name.Trim();
        warehouse.Code = request.Code.Trim().ToUpperInvariant();
        warehouse.ContactPerson = request.ContactPerson;
        warehouse.PhoneNumber = request.PhoneNumber;
        warehouse.Email = request.Email;
        warehouse.StreetLine1 = request.StreetLine1.Trim();
        warehouse.StreetLine2 = request.StreetLine2;
        warehouse.City = request.City.Trim();
        warehouse.StateOrProvince = request.StateOrProvince.Trim();
        warehouse.PostalCode = request.PostalCode.Trim();
        warehouse.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
        warehouse.IsActive = request.IsActive;
        warehouse.IsPrimary = request.IsPrimary;

        _warehouseRepo.Update(warehouse);
        await _warehouseRepo.SaveChangesAsync(cancellationToken);

        return await GetWarehouseByIdAsync(warehouse.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepo.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Warehouse not found.");
        }

        _warehouseRepo.Delete(warehouse);
        await _warehouseRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Warehouse deleted successfully.");
    }

    private static WarehouseDto MapToDto(Warehouse w)
    {
        return new WarehouseDto
        {
            Id = w.Id,
            Name = w.Name,
            Code = w.Code,
            ContactPerson = w.ContactPerson,
            PhoneNumber = w.PhoneNumber,
            Email = w.Email,
            StreetLine1 = w.StreetLine1,
            StreetLine2 = w.StreetLine2,
            City = w.City,
            StateOrProvince = w.StateOrProvince,
            PostalCode = w.PostalCode,
            CountryCode = w.CountryCode,
            IsActive = w.IsActive,
            IsPrimary = w.IsPrimary,
            TotalStockItems = w.InventoryItems.Count
        };
    }
}
