// GET /api/collections | POST /api/collections | GET /api/collections/{id} | DELETE /api/collections/{id}
using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Services;

public class CollectionService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<CollectionResponse>> GetAllAsync(int page, int pageSize)
    {
        DbSet<Collection> collections = _context.Collections ?? throw new InvalidOperationException("Collections DbSet is not configured.");

        int total = await collections.CountAsync();
        List<CollectionResponse> items = await collections
            .AsNoTracking()
            .Include(c => c.CollectionPoint)
            .Include(c => c.WasteType)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CollectionResponse
            {
                Id = c.Id,
                CollectionPointId = c.CollectionPointId,
                CollectionPointName = c.CollectionPoint.Name,
                WasteTypeId = c.WasteTypeId,
                WasteCategory = c.WasteType.WasteCategory,
                CollectedAt = c.CollectedAt,
                VolumeKg = c.VolumeKg,
                Status = c.Status
            })
            .ToListAsync();

        return new PagedResponse<CollectionResponse>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<CollectionResponse> GetByIdAsync(int id)
    {
        DbSet<Collection> collections = _context.Collections ?? throw new InvalidOperationException("Collections DbSet is not configured.");

        Collection c = await collections
            .AsNoTracking()
            .Include(c => c.CollectionPoint)
            .Include(c => c.WasteType)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Collection", id);

        return new CollectionResponse
        {
            Id = c.Id,
            CollectionPointId = c.CollectionPointId,
            CollectionPointName = c.CollectionPoint.Name,
            WasteTypeId = c.WasteTypeId,
            WasteCategory = c.WasteType.WasteCategory,
            CollectedAt = c.CollectedAt,
            VolumeKg = c.VolumeKg,
            Status = c.Status
        };
    }

    public async Task<CollectionResponse> CreateAsync(CollectionRequest request)
    {
        DbSet<CollectionPoint> points = _context.CollectionPoints ?? throw new InvalidOperationException("CollectionPoints DbSet is not configured.");
        DbSet<WasteType> wasteTypes = _context.WasteTypes ?? throw new InvalidOperationException("WasteTypes DbSet is not configured.");
        DbSet<Collection> collections = _context.Collections ?? throw new InvalidOperationException("Collections DbSet is not configured.");
        DbSet<CollectionAlert> alerts = _context.CollectionAlerts ?? throw new InvalidOperationException("CollectionAlerts DbSet is not configured.");

        CollectionPoint point = await points.FindAsync(request.CollectionPointId)
            ?? throw new NotFoundException("CollectionPoint", request.CollectionPointId);

        if (!await wasteTypes.AnyAsync(w => w.Id == request.WasteTypeId))
        {
            throw new NotFoundException("WasteType", request.WasteTypeId);
        }

        point.OccupiedVolumeKg += request.VolumeKg;
        point.UpdatedAt = DateTime.UtcNow;
        point.Status = point.OccupiedVolumeKg >= point.CapacityKg ? "CRITICAL"
            : point.OccupiedVolumeKg >= point.AlertVolumeKg ? "NEAR_LIMIT"
            : "AVAILABLE";

        Collection collection = new()
        {
            CollectionPointId = request.CollectionPointId,
            WasteTypeId = request.WasteTypeId,
            VolumeKg = request.VolumeKg,
            CollectedAt = DateTime.UtcNow,
            Status = "OPEN"
        };

        _ = collections.Add(collection);

        if (point.Status is "CRITICAL" or "NEAR_LIMIT")
        {
            _ = alerts.Add(new CollectionAlert
            {
                CollectionPointId = point.Id,
                AlertType = point.Status == "CRITICAL" ? "CAPACITY_EXCEEDED" : "LIMIT_REACHED",
                Message = $"CollectionPoint '{point.Name}' is at {point.OccupiedVolumeKg}kg of {point.CapacityKg}kg.",
                AlertedAt = DateTime.UtcNow
            });
        }

        _ = await _context.SaveChangesAsync();

        return await GetByIdAsync(collection.Id);
    }

    public async Task DeleteAsync(int id)
    {
        DbSet<Collection> collections = _context.Collections ?? throw new InvalidOperationException("Collections DbSet is not configured.");

        Collection collection = await collections.FindAsync(id)
            ?? throw new NotFoundException("Collection", id);

        _ = collections.Remove(collection);
        _ = await _context.SaveChangesAsync();
    }
}