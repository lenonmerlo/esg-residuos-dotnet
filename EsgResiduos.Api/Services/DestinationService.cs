// GET /api/destinations | POST /api/destinations | GET /api/destinations/{id}
using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Services;

public class DestinationService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<DestinationResponse>> GetAllAsync(int page, int pageSize)
    {
        DbSet<Destination> destinations = _context.Destinations ?? throw new InvalidOperationException("Destinations DbSet is not configured.");

        int total = await destinations.CountAsync();
        List<DestinationResponse> items = await destinations
            .AsNoTracking()
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
        DbSet<Destination> destinations = _context.Destinations ?? throw new InvalidOperationException("Destinations DbSet is not configured.");

        Destination d = await destinations.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException("Destination", id);

        return new DestinationResponse
        {
            Id = d.Id,
            CollectionId = d.CollectionId,
            DestinatedAt = d.DestinatedAt,
            DestinationName = d.DestinationName,
            ProcessingType = d.ProcessingType,
            DestinatedVolumeKg = d.DestinatedVolumeKg
        };
    }

    public async Task<DestinationResponse> CreateAsync(DestinationRequest request)
    {
        DbSet<Collection> collections = _context.Collections ?? throw new InvalidOperationException("Collections DbSet is not configured.");
        DbSet<Destination> destinations = _context.Destinations ?? throw new InvalidOperationException("Destinations DbSet is not configured.");

        Collection collection = await collections.FindAsync(request.CollectionId)
            ?? throw new NotFoundException("Collection", request.CollectionId);

        if (collection.Status == "DESTINATED")
        {
            throw new AppException("Collection is already destinated.", 400);
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
        collection.DestinationHistory = $"Destinated to {request.DestinationName} via {request.ProcessingType}";

        _ = destinations.Add(destination);
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