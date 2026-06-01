using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Services;

public class CollectionPointService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<CollectionPointResponse>> GetAllAsync(int page, int pageSize)
    {
        DbSet<CollectionPoint> points = _context.CollectionPoints ?? throw new InvalidOperationException("CollectionPoints DbSet is not configured.");

        int total = await points.CountAsync();
        List<CollectionPointResponse> items = await points
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
        DbSet<CollectionPoint> points = _context.CollectionPoints ?? throw new InvalidOperationException("CollectionPoints DbSet is not configured.");

        CollectionPoint point = await points.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("CollectionPoint", id);

        return MapToResponse(point);
    }

    public async Task<CollectionPointResponse> CreateAsync(CollectionPointRequest request)
    {
        DbSet<CollectionPoint> points = _context.CollectionPoints ?? throw new InvalidOperationException("CollectionPoints DbSet is not configured.");

        CollectionPoint point = new()
        {
            Name = request.Name,
            CapacityKg = request.CapacityKg,
            AlertVolumeKg = request.AlertVolumeKg,
            OccupiedVolumeKg = 0,
            Status = "AVAILABLE",
            UpdatedAt = DateTime.UtcNow
        };

        _ = points.Add(point);
        _ = await _context.SaveChangesAsync();

        return MapToResponse(point);
    }

    public async Task<CollectionPointResponse> UpdateAsync(int id, CollectionPointRequest request)
    {
        DbSet<CollectionPoint> points = _context.CollectionPoints ?? throw new InvalidOperationException("CollectionPoints DbSet is not configured.");

        CollectionPoint point = await points.FindAsync(id)
            ?? throw new NotFoundException("CollectionPoint", id);

        point.Name = request.Name;
        point.CapacityKg = request.CapacityKg;
        point.AlertVolumeKg = request.AlertVolumeKg;
        point.UpdatedAt = DateTime.UtcNow;

        _ = await _context.SaveChangesAsync();

        return MapToResponse(point);
    }

    public async Task DeleteAsync(int id)
    {
        DbSet<CollectionPoint> points = _context.CollectionPoints ?? throw new InvalidOperationException("CollectionPoints DbSet is not configured.");

        CollectionPoint point = await points.FindAsync(id)
            ?? throw new NotFoundException("CollectionPoint", id);

        _ = points.Remove(point);
        _ = await _context.SaveChangesAsync();
    }

    private static CollectionPointResponse MapToResponse(CollectionPoint p)
    {
        return new()
        {
            Id = p.Id,
            Name = p.Name,
            CapacityKg = p.CapacityKg,
            AlertVolumeKg = p.AlertVolumeKg,
            OccupiedVolumeKg = p.OccupiedVolumeKg,
            Status = p.Status,
            UpdatedAt = p.UpdatedAt
        };
    }
}