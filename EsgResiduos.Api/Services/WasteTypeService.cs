using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Services;

public class WasteTypeService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<WasteTypeResponse>> GetAllAsync(int page, int pageSize)
    {
        DbSet<WasteType> wasteTypes = _context.WasteTypes ?? throw new InvalidOperationException("WasteTypes DbSet is not configured.");

        int total = await wasteTypes.CountAsync();
        List<WasteTypeResponse> items = await wasteTypes
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WasteTypeResponse
            {
                Id = w.Id,
                WasteCategory = w.WasteCategory,
                Description = w.Description
            })
            .ToListAsync();

        return new PagedResponse<WasteTypeResponse>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<WasteTypeResponse> GetByIdAsync(int id)
    {
        DbSet<WasteType> wasteTypes = _context.WasteTypes ?? throw new InvalidOperationException("WasteTypes DbSet is not configured.");

        WasteType waste = await wasteTypes.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new NotFoundException("WasteType", id);

        return new WasteTypeResponse
        {
            Id = waste.Id,
            WasteCategory = waste.WasteCategory,
            Description = waste.Description
        };
    }

    public async Task<WasteTypeResponse> CreateAsync(WasteTypeRequest request)
    {
        DbSet<WasteType> wasteTypes = _context.WasteTypes ?? throw new InvalidOperationException("WasteTypes DbSet is not configured.");

        if (await wasteTypes.AnyAsync(w => w.WasteCategory == request.WasteCategory))
        {
            throw new AppException("WasteCategory already exists.", 409);
        }

        WasteType waste = new()
        {
            WasteCategory = request.WasteCategory,
            Description = request.Description
        };

        _ = wasteTypes.Add(waste);
        _ = await _context.SaveChangesAsync();

        return new WasteTypeResponse
        {
            Id = waste.Id,
            WasteCategory = waste.WasteCategory,
            Description = waste.Description
        };
    }

    public async Task<WasteTypeResponse> UpdateAsync(int id, WasteTypeRequest request)
    {
        DbSet<WasteType> wasteTypes = _context.WasteTypes ?? throw new InvalidOperationException("WasteTypes DbSet is not configured.");

        WasteType waste = await wasteTypes.FindAsync(id)
            ?? throw new NotFoundException("WasteType", id);

        waste.WasteCategory = request.WasteCategory;
        waste.Description = request.Description;

        _ = await _context.SaveChangesAsync();

        return new WasteTypeResponse
        {
            Id = waste.Id,
            WasteCategory = waste.WasteCategory,
            Description = waste.Description
        };
    }

    public async Task DeleteAsync(int id)
    {
        DbSet<WasteType> wasteTypes = _context.WasteTypes ?? throw new InvalidOperationException("WasteTypes DbSet is not configured.");

        WasteType waste = await wasteTypes.FindAsync(id)
            ?? throw new NotFoundException("WasteType", id);

        _ = wasteTypes.Remove(waste);
        _ = await _context.SaveChangesAsync();
    }
}