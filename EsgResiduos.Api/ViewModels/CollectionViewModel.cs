using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.ViewModels;

public class CollectionViewModel(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<CollectionResponse>> GetAllAsync(int page, int pageSize)
    {
        int total = await _context.Collections.CountAsync();
        List<CollectionResponse> items = await _context.Collections
            .AsNoTracking()
            .Include(c => c.CollectionPoint)
            .Include(c => c.WasteType)
            .OrderBy(c => c.Id)
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
        Collection collection = await _context.Collections
            .AsNoTracking()
            .Include(c => c.CollectionPoint)
            .Include(c => c.WasteType)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Coleta", id);

        return new CollectionResponse
        {
            Id = collection.Id,
            CollectionPointId = collection.CollectionPointId,
            CollectionPointName = collection.CollectionPoint.Name,
            WasteTypeId = collection.WasteTypeId,
            WasteCategory = collection.WasteType.WasteCategory,
            CollectedAt = collection.CollectedAt,
            VolumeKg = collection.VolumeKg,
            Status = collection.Status
        };
    }

    public async Task<CollectionResponse> CreateAsync(CollectionRequest request)
    {
        CollectionPoint point = await _context.CollectionPoints.FindAsync(request.CollectionPointId)
            ?? throw new NotFoundException("Ponto de coleta", request.CollectionPointId);

        if (!await _context.WasteTypes.AnyAsync(w => w.Id == request.WasteTypeId))
        {
            throw new NotFoundException("Tipo de resíduo", request.WasteTypeId);
        }

        point.OccupiedVolumeKg += request.VolumeKg;
        point.UpdatedAt = DateTime.UtcNow;

        // Regra ESG: recalcula ocupação do ponto e classifica o status para disparo de alerta.
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

        _ = _context.Collections.Add(collection);

        // Alerta automático quando o ponto atinge o volume configurado em AlertVolumeKg.
        if (point.Status is "CRITICAL" or "NEAR_LIMIT")
        {
            _ = _context.CollectionAlerts.Add(new CollectionAlert
            {
                CollectionPointId = point.Id,
                AlertType = point.Status == "CRITICAL" ? "CAPACITY_EXCEEDED" : "LIMIT_REACHED",
                Message = $"Ponto '{point.Name}' com {point.OccupiedVolumeKg}kg de {point.CapacityKg}kg.",
                AlertedAt = DateTime.UtcNow
            });
        }

        _ = await _context.SaveChangesAsync();
        return await GetByIdAsync(collection.Id);
    }

    public async Task DeleteAsync(int id)
    {
        Collection collection = await _context.Collections.FindAsync(id)
            ?? throw new NotFoundException("Coleta", id);

        List<Destination> destinations = await _context.Destinations
            .Where(d => d.CollectionId == id)
            .ToListAsync();

        if (destinations.Count > 0)
        {
            _context.Destinations.RemoveRange(destinations);
        }

        List<CollectionAlert> alerts = await _context.CollectionAlerts
            .Where(a => a.CollectionId == id)
            .ToListAsync();

        foreach (CollectionAlert alert in alerts)
        {
            alert.CollectionId = null;
        }

        _ = _context.Collections.Remove(collection);
        _ = await _context.SaveChangesAsync();
    }
}
