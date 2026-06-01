// GET /api/collectionalerts | GET /api/collectionalerts/{id}
using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Services;

public class CollectionAlertService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResponse<CollectionAlertResponse>> GetAllAsync(int page, int pageSize)
    {
        DbSet<CollectionAlert> alerts = _context.CollectionAlerts ?? throw new InvalidOperationException("CollectionAlerts DbSet is not configured.");

        int total = await alerts.CountAsync();
        List<CollectionAlertResponse> items = await alerts
            .AsNoTracking()
            .Include(a => a.CollectionPoint)
            .OrderByDescending(a => a.AlertedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new CollectionAlertResponse
            {
                Id = a.Id,
                CollectionPointId = a.CollectionPointId,
                CollectionPointName = a.CollectionPoint.Name,
                CollectionId = a.CollectionId,
                AlertedAt = a.AlertedAt,
                AlertType = a.AlertType,
                Message = a.Message
            })
            .ToListAsync();

        return new PagedResponse<CollectionAlertResponse>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<CollectionAlertResponse> GetByIdAsync(int id)
    {
        DbSet<CollectionAlert> alerts = _context.CollectionAlerts ?? throw new InvalidOperationException("CollectionAlerts DbSet is not configured.");

        CollectionAlert alert = await alerts
            .AsNoTracking()
            .Include(a => a.CollectionPoint)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("CollectionAlert", id);

        return new CollectionAlertResponse
        {
            Id = alert.Id,
            CollectionPointId = alert.CollectionPointId,
            CollectionPointName = alert.CollectionPoint.Name,
            CollectionId = alert.CollectionId,
            AlertedAt = alert.AlertedAt,
            AlertType = alert.AlertType,
            Message = alert.Message
        };
    }
}