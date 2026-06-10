using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.ViewModels;

public class CollectionPointViewModel(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<CollectionPointResponse>> GetAllAsync(int page, int pageSize)
    {
        int total = await _context.CollectionPoints.CountAsync();
        List<CollectionPointResponse> items = await _context.CollectionPoints
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new CollectionPointResponse
            {
                Id = p.Id,
                Name = p.Name,
                CapacityKg = p.CapacityKg,
                AlertVolumeKg = p.AlertVolumeKg,
                OccupiedVolumeKg = p.OccupiedVolumeKg,
                Status = p.Status,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<CollectionPointResponse>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<CollectionPointResponse> GetByIdAsync(int id)
    {
        CollectionPoint point = await _context.CollectionPoints.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Ponto de coleta", id);

        return MapToResponse(point);
    }

    public async Task<CollectionPointResponse> CreateAsync(CollectionPointRequest request)
    {
        CollectionPoint point = new()
        {
            Name = request.Name,
            CapacityKg = request.CapacityKg,
            AlertVolumeKg = request.AlertVolumeKg,
            OccupiedVolumeKg = 0,
            Status = "AVAILABLE",
            UpdatedAt = DateTime.UtcNow
        };

        _ = _context.CollectionPoints.Add(point);
        _ = await _context.SaveChangesAsync();

        return MapToResponse(point);
    }

    public async Task<CollectionPointResponse> UpdateAsync(int id, CollectionPointRequest request)
    {
        CollectionPoint point = await _context.CollectionPoints.FindAsync(id)
            ?? throw new NotFoundException("Ponto de coleta", id);

        point.Name = request.Name;
        point.CapacityKg = request.CapacityKg;
        point.AlertVolumeKg = request.AlertVolumeKg;
        point.UpdatedAt = DateTime.UtcNow;
        _ = await _context.SaveChangesAsync();

        return MapToResponse(point);
    }

    public async Task DeleteAsync(int id)
    {
        CollectionPoint point = await _context.CollectionPoints.FindAsync(id)
            ?? throw new NotFoundException("Ponto de coleta", id);

        bool hasCollections = await _context.Collections
            .AnyAsync(c => c.CollectionPointId == id);

        if (hasCollections)
        {
            throw new AppException("Não é possível excluir ponto de coleta com coletas vinculadas.", 409);
        }

        List<CollectionAlert> alerts = await _context.CollectionAlerts
            .Where(a => a.CollectionPointId == id)
            .ToListAsync();

        if (alerts.Count > 0)
        {
            _context.CollectionAlerts.RemoveRange(alerts);
        }

        _ = _context.CollectionPoints.Remove(point);
        _ = await _context.SaveChangesAsync();
    }

    private static CollectionPointResponse MapToResponse(CollectionPoint point)
    {
        return new CollectionPointResponse
        {
            Id = point.Id,
            Name = point.Name,
            CapacityKg = point.CapacityKg,
            AlertVolumeKg = point.AlertVolumeKg,
            OccupiedVolumeKg = point.OccupiedVolumeKg,
            Status = point.Status,
            UpdatedAt = point.UpdatedAt
        };
    }
}
