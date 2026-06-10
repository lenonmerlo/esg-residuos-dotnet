using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.ViewModels;

public class WasteTypeViewModel(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<WasteTypeResponse>> GetAllAsync(int page, int pageSize)
    {
        int total = await _context.WasteTypes.CountAsync();
        List<WasteTypeResponse> items = await _context.WasteTypes
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
        WasteType waste = await _context.WasteTypes.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new NotFoundException("Tipo de resíduo", id);

        return new WasteTypeResponse
        {
            Id = waste.Id,
            WasteCategory = waste.WasteCategory,
            Description = waste.Description
        };
    }

    public async Task<WasteTypeResponse> CreateAsync(WasteTypeRequest request)
    {
        if (await _context.WasteTypes.AnyAsync(w => w.WasteCategory == request.WasteCategory))
        {
            throw new AppException("Categoria de resíduo já cadastrada.", 409);
        }

        WasteType waste = new()
        {
            WasteCategory = request.WasteCategory,
            Description = request.Description
        };

        _ = _context.WasteTypes.Add(waste);
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
        WasteType waste = await _context.WasteTypes.FindAsync(id)
            ?? throw new NotFoundException("Tipo de resíduo", id);

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
        WasteType waste = await _context.WasteTypes.FindAsync(id)
            ?? throw new NotFoundException("Tipo de resíduo", id);

        bool hasCollections = await _context.Collections
            .AnyAsync(c => c.WasteTypeId == id);

        if (hasCollections)
        {
            throw new AppException("Não é possível excluir tipo de resíduo com coletas vinculadas.", 409);
        }

        _ = _context.WasteTypes.Remove(waste);
        _ = await _context.SaveChangesAsync();
    }
}
