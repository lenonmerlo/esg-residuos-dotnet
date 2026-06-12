using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.ViewModels;

public class DestinationViewModel(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<DestinationResponse>> GetAllAsync(int page, int pageSize)
    {
        int total = await _context.Destinations.CountAsync();
        List<DestinationResponse> items = await _context.Destinations
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DestinationResponse
            {
                Id = d.Id,
                CollectionId = d.CollectionId,
                DestinatedAt = d.DestinatedAt,
                DestinationName = d.DestinationName,
                ProcessingType = d.ProcessingType,
                DestinatedVolumeKg = d.DestinatedVolumeKg
            })
            .ToListAsync();

        return new PagedResponse<DestinationResponse>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<DestinationResponse> GetByIdAsync(int id)
    {
        Destination destination = await _context.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException("Destinação", id);

        return new DestinationResponse
        {
            Id = destination.Id,
            CollectionId = destination.CollectionId,
            DestinatedAt = destination.DestinatedAt,
            DestinationName = destination.DestinationName,
            ProcessingType = destination.ProcessingType,
            DestinatedVolumeKg = destination.DestinatedVolumeKg
        };
    }

    public async Task<DestinationResponse> CreateAsync(DestinationRequest request)
    {
        Collection collection = await _context.Collections
            .Include(c => c.CollectionPoint)
            .Include(c => c.WasteType)
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId)
            ?? throw new NotFoundException("Coleta", request.CollectionId);

        if (collection.Status == "DESTINATED")
        {
            throw new AppException("Coleta já foi destinada.", 400);
        }

        Destination destination = new()
        {
            CollectionId = request.CollectionId,
            DestinationName = request.DestinationName,
            ProcessingType = request.ProcessingType,
            DestinatedVolumeKg = request.DestinatedVolumeKg,
            DestinatedAt = DateTime.UtcNow
        };

        collection.Status = "DESTINATED";
        collection.DestinatedAt = DateTime.UtcNow;
        collection.DestinationHistory = $"Destinado para {request.DestinationName} via {request.ProcessingType}";

        _ = _context.Destinations.Add(destination);

        // Notifica o usuário sobre a destinação correta do resíduo (requisito do tema ESG).
        _ = _context.CollectionAlerts.Add(new CollectionAlert
        {
            CollectionPointId = collection.CollectionPointId,
            CollectionId = collection.Id,
            AlertType = "DESTINATION_NOTIFICATION",
            Message = $"Coleta de {collection.WasteType.WasteCategory} ({collection.VolumeKg}kg) destinada para '{request.DestinationName}' via {request.ProcessingType}.",
            AlertedAt = DateTime.UtcNow
        });

        _ = await _context.SaveChangesAsync();

        return new DestinationResponse
        {
            Id = destination.Id,
            CollectionId = destination.CollectionId,
            DestinatedAt = destination.DestinatedAt,
            DestinationName = destination.DestinationName,
            ProcessingType = destination.ProcessingType,
            DestinatedVolumeKg = destination.DestinatedVolumeKg
        };
    }
}
